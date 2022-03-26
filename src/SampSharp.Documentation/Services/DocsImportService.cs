// SampSharp.Documentation
// Copyright 2019 Tim Potze
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PlayNow.StarTar;
using PlayNow.StarTar.Headers;
using SampSharp.Documentation.Configuration;
using SampSharp.Documentation.Markdown.Renderers;
using SampSharp.Documentation.Models;
using SampSharp.Documentation.NewModels;
using SampSharp.Documentation.Repositories;
using DocVersion = SampSharp.Documentation.Models.DocVersion;

namespace SampSharp.Documentation.Services
{
	public class DocsImportService : IDocsImportService
	{
		private static readonly char[] LineBreaks = { '\r', '\n' };
		private readonly IDocumentationDataRepository _documentationDataRepository;
		private readonly IOptions<ImportOptions> _options;
		private readonly ILogger<DocsImportService> _logger;
		private readonly IGithubDataRepository _githubDataRepository;

		public DocsImportService(IGithubDataRepository githubDataRepository, IDocumentationDataRepository documentationDataRepository, IOptions<ImportOptions> options, ILogger<DocsImportService> logger)
		{
			_githubDataRepository = githubDataRepository;
			_documentationDataRepository = documentationDataRepository;
			_options = options;
			_logger = logger;
		}

		private static string BranchNameToPathString(string branch) //TODO: Move to some string util
		{
			return branch?.Replace("/", "---").Replace("\\", "-----");
		}

		private int IndexOfNoWhiteSpace(string input, int startIndex)
		{
			if (input == null)
				throw new ArgumentNullException(nameof(input));

			for (var i = startIndex; i < input.Length; i++)
			{
				if (!char.IsWhiteSpace(input[i]))
					return i;
			}

			return -1;
		}

		private int IndexAfterAny(string input, char[] anyChar, int startIndex)
		{
			if (input == null)
				throw new ArgumentNullException(nameof(input));
			if (anyChar == null)
				throw new ArgumentNullException(nameof(anyChar));

			for (var i = startIndex; i < input.Length; i++)
			{
				if (Array.IndexOf(anyChar, input[i]) == -1)
					return i;
			}

			return -1;
		}

		private DocMeta ParseMeta(ref string markdown)
		{
			// TODO: Markdig can do this for us.
			var dataLines = new List<string>();
			var endOfEndMarker = -1;

			// Find marker
			var positionOfMarker = IndexOfNoWhiteSpace(markdown, 0);
			if (positionOfMarker >= 0 && positionOfMarker < markdown.Length && markdown.Substring(positionOfMarker, 3) == "---")
			{
				var endOfStartMarker = IndexAfterAny(markdown, new[] { '-' }, positionOfMarker);

				// Line break after marker
				if (Array.IndexOf(LineBreaks, markdown[endOfStartMarker]) >= 0)
				{
					var startOfDataLine = IndexOfNoWhiteSpace(markdown, endOfStartMarker);

					while (startOfDataLine >= 0)
					{
						var endOfDataLine = markdown.IndexOfAny(LineBreaks, startOfDataLine);

						if (endOfDataLine < 0)
							break;

						var line = markdown[startOfDataLine..endOfDataLine].Trim();

						startOfDataLine = IndexOfNoWhiteSpace(markdown, endOfDataLine);

						if (line.Length >= 3 && line.All(c => c == '-'))
						{
							endOfEndMarker = startOfDataLine;
							if (endOfEndMarker >= 0)
							{
								endOfEndMarker = IndexAfterAny(markdown, new[] { '-' }, endOfEndMarker);
								endOfEndMarker = IndexOfNoWhiteSpace(markdown, endOfEndMarker);
							}
							else
								endOfEndMarker = markdown.Length;

							break;
						}

						if (line.Length > 0)
							dataLines.Add(line);
					}
				}
			}

			// Read metadata
			var meta = new DocMeta();

			foreach (var dataLine in dataLines)
			{
				var lineSplit = dataLine.Split(':', 2);

				var key = lineSplit[0].Trim();
				var value = lineSplit[1].Trim();

				switch (key)
				{
					case "title":
						meta.Title = value;
						break;
					case "redirect_to":
						if (value.StartsWith("http://") || value.StartsWith("https://"))
							meta.RedirectUrl = value;
						else
							meta.RedirectPage = value;
						break;
				}
			}

			// Trim metadata from markdown text
			if (endOfEndMarker > 0)
			{
				markdown = endOfEndMarker < markdown.Length
					? markdown.Substring(endOfEndMarker)
					: string.Empty;
			}

			return meta;
		}

		public async Task ImportAllBranches()
		{
			_logger.LogInformation("Importing all branches");
			var branches = await _githubDataRepository.GetBranches();

			_logger.LogInformation($"Collected {branches.Length} branches to import");

			var versions = new List<DocVersion>();
			DocVersion defaultVersion = null;
			foreach (var branch in branches)
			{
				_logger.LogInformation($"Processing branch {branch.Name}");

				_documentationDataRepository.DeleteBranch(branch.Name);

				var version = new DocVersion(BranchNameToPathString(branch.Name), branch.Name);

				versions.Add(version);
				if (branch.IsDefault)
					defaultVersion = version;

				var archive = await _githubDataRepository.GetArchive(branch);

				await using var memoryStream = new MemoryStream(archive);
				await ImportBranch(memoryStream, version, branch);
			}


			_logger.LogInformation("Done processing, storing to data store");

			var docConfig = new DocConfiguration
			{
				Versions = versions.ToArray(),
				DefaultVersion = defaultVersion
			};

			_documentationDataRepository.StoreDocConfiguration(docConfig);
		}

		private async Task ImportBranch(MemoryStream memoryStream, DocVersion version, GithubBranch branch)
		{
			await using var gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress);
			using var tarReader = new TarReader(gzipStream);
			var treeBuilder = new MenuTreeBuilder();

			string rootDirectory = null;
			while (tarReader.Next() is var entry && entry != null)
			{
				switch (entry.Header.Flag)
				{
					case TarHeaderFlag.Directory when rootDirectory == null:
						rootDirectory = entry.Header.Name;
						break;
					case TarHeaderFlag.NormalFile when Path.GetExtension(entry.Header.Name) == ".md":
					{
						ImportDoc(version, branch, entry, treeBuilder, rootDirectory);
						break;
					}
					case TarHeaderFlag.NormalFile:
					{
						ImportAsset(branch, entry, rootDirectory);

						break;
					}
				}
			}

			version.Menu = treeBuilder.Build();
		}

		private void ImportDoc(DocVersion version, GithubBranch branch, TarEntryStream entry, MenuTreeBuilder treeBuilder, string rootDirectory)
		{
			using var streamReader = new StreamReader(entry);
			var markdown = streamReader.ReadToEnd();

			var meta = ParseMeta(ref markdown);

			// Parse markdown
			var document = Markdig.Markdown.Parse(markdown,
				new MarkdownPipelineBuilder()
					.UseAdvancedExtensions()
					.UsePipeTables()
					.Build());

			// Remove old-style preamble (ul with headings links, introduction heading)
			if (document.Count > 1 && !(document[0] is ParagraphBlock))
			{
				var introHead = document
					.Select((block, index) => new {block, index})
					.FirstOrDefault(x =>
						x.block is HeadingBlock h && h.Inline.FindDescendants<LiteralInline>()?.FirstOrDefault()?.Content.ToString() == "Introduction");

				if (introHead != null && document.Count > introHead.index + 1 && document[introHead.index + 1] is ParagraphBlock)
				{
					for (var i = introHead.index; i >= 0; i--) document.RemoveAt(i);
				}
			}

			// Collect introduction
			var sw = new StringWriter();
			while (document.Count > 0 && !(document[0] is HeadingBlock))
			{
				// Render intro
				Render(document[0], sw);
				document.RemoveAt(0);
			}

			var rendered = sw.ToString();
			meta.Introduction = string.IsNullOrWhiteSpace(rendered) ? null : rendered;

			// Collect quick links
			meta.QuickLinks = new List<ArticleQuickLink>();

			foreach (var x in document)
			{
				if (x is HeadingBlock head && head.Level == 2)
				{
					var text = head.Inline?.FirstChild?.ToString();

					if (text != null)
					{
						meta.QuickLinks.Add(new ArticleQuickLink
						{
							Link = $"#{CustomHeadingRenderer.EscapeName(text)}",
							Name = text
						});
					}
				}
			}

			// Render remainder of document
			var html = Render(document);

			var file = new DocFile
			{
				Content = html,
				Meta = meta
			};

			// Compute path of file
			var docPath = entry.Header.Name;

			if (rootDirectory != null && docPath.StartsWith(rootDirectory))
				docPath = docPath.Substring(rootDirectory.Length).TrimStart('/', '\\');

			var fileName = Path.GetFileNameWithoutExtension(docPath);
			docPath = Path.Combine(Path.GetDirectoryName(docPath), fileName);

			// Patch version metadata
			if (fileName == "index") version.DefaultPage = meta.RedirectPage;

			// Updated metadata based on file entry
			meta.LastModification = entry.Header.LastModification ?? DateTime.UtcNow;
			meta.EditUrl = $"{_githubDataRepository.PublicUrl}/blob/{branch.Name}/{docPath}.md";
			meta.Title ??= fileName.Replace('-', ' ');

			var linkInfo = new List<LinkInfo>();

			foreach(var p in docPath)
			meta.Breadcrumbs = linkInfo.ToArray();

			treeBuilder.AddPage(docPath, meta.Title, "docs", new {version=version.Tag, page=docPath});
			_documentationDataRepository.StoreDocFile(branch.Name, docPath, file);
		}

		private void ImportAsset(GithubBranch branch, TarEntryStream entry, string rootDirectory)
		{
			var assetPath = entry.Header.Name;

			if (rootDirectory != null && assetPath.StartsWith(rootDirectory))
				assetPath = assetPath.Substring(rootDirectory.Length).TrimStart('/', '\\');

			assetPath = assetPath.ToLower();

			// Skip unaccepted asset types
			if (_options.Value.AcceptedAssets.Contains(Path.GetExtension(assetPath)))
			{
				_documentationDataRepository.StoreAsset(branch.Name, assetPath, entry);
			}
		}

		private string Render(MarkdownObject markdownObject)
		{
			var sw = new StringWriter();
			Render(markdownObject, sw);
			return sw.ToString();
		}

		private void Render(MarkdownObject markdownObject, TextWriter writer)
		{
			var renderer = new CustomHtmlRenderer(writer);
			renderer.Render(markdownObject);
			writer.Flush();
		}
	}
}
