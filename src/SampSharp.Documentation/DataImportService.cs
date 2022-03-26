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

using System.IO.Compression;
using Markdig;
using Microsoft.Extensions.Options;
using PlayNow.StarTar;
using PlayNow.StarTar.Headers;
using SampSharp.Documentation.Configuration;
using SampSharp.Documentation.Markdown.Renderers;
using SampSharp.Documentation.Models;
using SampSharp.Documentation.Repositories;

namespace SampSharp.Documentation
{
	public class DataImportService : IDataImportService
	{
		private static readonly char[] LineBreaks = { '\r', '\n' };
		private readonly IDataRepository _dataRepository;
		private readonly IOptions<ImportOptions> _options;
		private readonly IGithubDataRepository _githubDataRepository;

		public DataImportService(IGithubDataRepository githubDataRepository, IDataRepository dataRepository, IOptions<ImportOptions> options)
		{
			_githubDataRepository = githubDataRepository;
			_dataRepository = dataRepository;
			_options = options;
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

						var line = markdown.Substring(startOfDataLine, endOfDataLine - startOfDataLine).Trim();

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
			var branches = await _githubDataRepository.GetBranches();

			var versions = new List<DocVersion>();
			DocVersion defaultVersion = null;
			foreach (var branch in branches)
			{
				_dataRepository.DeleteBranch(branch.Name);

				var version = new DocVersion(BranchNameToPathString(branch.Name), branch.Name);

				versions.Add(version);
				if (branch.IsDefault)
					defaultVersion = version;

				var archive = await _githubDataRepository.GetArchive(branch);

				using (var memoryStream = new MemoryStream(archive))
				using (var gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
				using (var tarReader = new TarReader(gzipStream))
				{
					string rootDirectory = null;
					while (tarReader.Next() is var entry && entry != null)
					{
						if (entry.Header.Flag == TarHeaderFlag.Directory && rootDirectory == null) rootDirectory = entry.Header.Name;
						if (entry.Header.Flag == TarHeaderFlag.NormalFile)
						{
							if (Path.GetExtension(entry.Header.Name) == ".md")
							{
								using (var streamReader = new StreamReader(entry))
								{
									var markdown = streamReader.ReadToEnd();

									var meta = ParseMeta(ref markdown);

									// Parse markdown
									var document = Markdig.Markdown.Parse(markdown,
										new MarkdownPipelineBuilder()
											.UseAdvancedExtensions()
											.UsePipeTables()
											.Build());

									var sw = new StringWriter();
									var renderer = new CustomHtmlRenderer(sw);
									renderer.Render(document);
									sw.Flush();
									var html = sw.ToString();

									var file = new DocFile
									{
										Content = html,
										Meta = meta
									};

									var docPath = entry.Header.Name;

									if (rootDirectory != null && docPath.StartsWith(rootDirectory))
										docPath = docPath.Substring(rootDirectory.Length).TrimStart('/', '\\');

									var fileName = Path.GetFileNameWithoutExtension(docPath);
									docPath = Path.Combine(Path.GetDirectoryName(docPath), fileName);

									if (fileName == "index") version.DefaultPage = meta.RedirectPage;

									meta.LastModification = entry.Header.LastModification ?? DateTime.UtcNow;
									meta.EditUrl = $"{_githubDataRepository.PublicUrl}/blob/{branch.Name}/{docPath}.md";
									meta.Title = meta.Title ?? fileName.Replace('-', ' ');

									_dataRepository.StoreDocFile(branch.Name, docPath, file);
								}
							}
							else
							{
								var assetPath = entry.Header.Name;

								if (rootDirectory != null && assetPath.StartsWith(rootDirectory))
									assetPath = assetPath.Substring(rootDirectory.Length).TrimStart('/', '\\');

								assetPath = assetPath.ToLower();

								// Skip unaccepted asset types
								if (_options.Value.AcceptedAssets.Contains(Path.GetExtension(assetPath)))
								{
									_dataRepository.StoreAsset(branch.Name, assetPath, entry);
								}

							}
						}
					}
				}
			}

			var docConfig = new DocConfiguration
			{
				Versions = versions.ToArray(),
				DefaultVersion = defaultVersion
			};

			_dataRepository.StoreDocConfiguration(docConfig);
		}
	}
}
