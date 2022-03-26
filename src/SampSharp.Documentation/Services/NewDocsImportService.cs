using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Markdig;
using Markdig.Extensions.Yaml;
using Markdig.Helpers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PlayNow.StarTar;
using PlayNow.StarTar.Headers;
using SampSharp.Documentation.Configuration;
using SampSharp.Documentation.Markdown.Renderers;
using SampSharp.Documentation.NewModels;
using SampSharp.Documentation.Repositories;

namespace SampSharp.Documentation.Services
{
	public class NewDocsImportService
	{
		private readonly IOptions<ImportOptions> _options;
		private readonly ILogger<DocsImportService> _logger;
		private readonly IGithubDataRepository _githubDataRepository;

		private readonly MarkdownPipeline _markdownPipeline = new MarkdownPipelineBuilder()
			.UseAdvancedExtensions()
			.UsePipeTables()
			.UseYamlFrontMatter()
			.Build();

		public NewDocsImportService(IGithubDataRepository githubDataRepository, IOptions<ImportOptions> options, ILogger<DocsImportService> logger)
		{
			_githubDataRepository = githubDataRepository;
			_options = options;
			_logger = logger;
		}

		private static string BranchNameToPathString(string branch) //TODO: Move to some string util
		{
			return branch?.Replace("/", "---").Replace("\\", "-----");
		}

		public async Task ImportAllBranches()
		{
			_logger.LogInformation("Importing all branches");
			var branches = await _githubDataRepository.GetBranches();

			_logger.LogInformation($"Collected {branches.Length} branches to import");

			foreach (var branch in branches)
			{
				_logger.LogInformation($"Processing branch {branch.Name}");

				var version = await ImportBranch(branch);

				if (version != null)
					ActivateVersion(version);
			}
		}

		public async Task ImportBranch(string branchName)
		{
			_logger.LogInformation($"Importing branch {branchName}");
			var branch = await _githubDataRepository.GetBranch(branchName);

			if (branch == null)
				return;

			_logger.LogInformation($"Processing branch {branch.Name}");

			var version = await ImportBranch(branch);

			if (version != null)
				ActivateVersion(version);
		}

		private void ActivateVersion(DocVersion version)
		{
			// todo
		}

		private async Task<DocVersion> ImportBranch(GithubBranch branch)
		{
			var result = new DocVersion();

			var archive = await _githubDataRepository.GetArchive(branch);
			await using var memoryStream = new MemoryStream(archive);

			await using var gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress);
			using var tarReader = new TarReader(gzipStream);

			result.Name = branch.Name;
			result.Uri = BranchNameToPathString(branch.Name);
			result.CreationDate = DateTime.UtcNow;
			result.IsActive = true;

			string rootDirectory = null;
			while (tarReader.Next() is var entry && entry != null)
			{
				var relativePath = entry.Header.Name;
				if (rootDirectory != null && relativePath.StartsWith(rootDirectory))
					relativePath = relativePath.Substring(rootDirectory.Length).TrimStart('/', '\\');

				switch (entry.Header.Flag)
				{
					case TarHeaderFlag.Directory when rootDirectory == null:
						rootDirectory = entry.Header.Name;
						break;
					case TarHeaderFlag.NormalFile when Path.GetExtension(entry.Header.Name) == ".md":
					{
						var article = ImportArticle(entry, branch.Name, relativePath);

						// consolidate category result.Categories
						break;
					}
					case TarHeaderFlag.NormalFile:
					{
						var asset = ImportAsset(entry, relativePath);

						if (asset != null)
						{
							result.Assets.Add(asset);
						}
						break;
					}
				}
			}

			return result;
		}

		private void TrimArticlePreamble(MarkdownDocument document)
		{
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
		}

		private IEnumerable<DocParagraph> ParseParagraphs(MarkdownDocument document)
		{
			foreach (var x in document)
			{
				if (x is HeadingBlock head && head.Level == 2)
				{
					var text = head.Inline?.FirstChild?.ToString();

					if (text != null)
					{
						yield return new DocParagraph
						{
							Name = text,
							Uri = $"#{CustomHeadingRenderer.EscapeName(text)}"
						};
					}
				}
			}
		}

		private Dictionary<string, string> ApplyMetadata(YamlFrontMatterBlock frontMatter, DocArticle article)
		{
			var result = new Dictionary<string, string>();

			foreach (StringLine line in frontMatter.Lines.OfType<StringLine>().Take(frontMatter.Lines.Count))
			{
				if (line.ToString().Length == 0)
					continue;
				var lineSplit = line.ToString().Split(':', 2);

				var key = lineSplit[0].Trim();
				var value = lineSplit[1].Trim();

				result[key] = value;
				switch (key)
				{
					case "title":
						article.Title = value;
						break;
					case "redirect_to":
						if (value.StartsWith("http://") || value.StartsWith("https://"))
							article.RedirectUrl = value;
						else
							article.RedirectPage = value;
						break;
					case "category":
						article.Category = new DocCategory
						{
							Name = value,
							Uri = BranchNameToPathString(value)
						};
						break;
				}
			}

			return result;
		}

		private DocArticle ImportArticle(TarEntryStream entry, string branchName, string relativePath)
		{
			var result = new DocArticle();

			// Parse the document
			using var streamReader = new StreamReader(entry);
			var markdown = streamReader.ReadToEnd();
			var document = Markdig.Markdown.Parse(markdown, _markdownPipeline);

			// Extract metadata
			if (document.Count > 0 && document[0] is YamlFrontMatterBlock frontMatter)
			{
				var meta = ApplyMetadata(frontMatter, result);

				if (meta.ContainsKey("ignore"))
					return null;

				document.RemoveAt(0);
			}
			
			// Remove unnecessary heading index list and "Introduction" heading
			TrimArticlePreamble(document);

			// Collect introduction which is all content up to the first heading
			var sw = new StringWriter();
			while (document.Count > 0 && !(document[0] is HeadingBlock))
			{
				Render(document[0], sw);
				document.RemoveAt(0);
			}

			result.Introduction = sw.ToString();
			if (string.IsNullOrWhiteSpace(result.Introduction))
				result.Introduction = null;
			
			result.Title ??= Path.GetFileNameWithoutExtension(relativePath);
			result.Uri = relativePath;
			result.LastModification = entry.Header.LastModification ?? DateTime.UtcNow;
			result.EditUrl = $"{_githubDataRepository.PublicUrl}/blob/{branchName}/{relativePath}.md";
			result.Content = Render(document);

			if (result.Category == null)
			{
				result.Category = new DocCategory
				{
					Name = "Docs",
					Uri = "docs"
				};
			}

			result.Paragraphs.AddRange(ParseParagraphs(document));
			foreach (var p in result.Paragraphs)
				p.Article = result;

			return result;
		}

		private DocAsset ImportAsset(TarEntryStream entry, string relativePath)
		{
			// Skip unaccepted asset types
			if (_options.Value.AcceptedAssets.Contains(Path.GetExtension(relativePath)))
			{
				using var memoryStream = new MemoryStream();
				entry.CopyTo(memoryStream);

				return new DocAsset
				{
					Uri = relativePath,
					Data = memoryStream.ToArray()
				};
			}

			return null;
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
