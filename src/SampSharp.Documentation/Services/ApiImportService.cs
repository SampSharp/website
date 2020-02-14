using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using DocPluck.Parser;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using SampSharp.Documentation.Models;
using Formatting = Newtonsoft.Json.Formatting;

namespace SampSharp.Documentation.Services
{
	public class ApiImportService
	{
		private readonly IViewRenderService _viewRenderService;

		public ApiImportService(IViewRenderService viewRenderService)
		{
			_viewRenderService = viewRenderService;
		}

		private readonly HttpClient _httpClient = new HttpClient();
		private readonly DocsParser _parser = new DocsParser();

		public async Task ImportAllFromNuGet()
		{
			foreach (var package in await DiscoverPackages())
			{
				await ParsePackage(package);
			}
		}

		private async Task ParsePackage(AvailablePackage package)
		{
			await using var stream = await _httpClient.GetStreamAsync(package.DownloadUrl);

			var parsed = _parser.ParseFromNugetPackage(stream);
			var parsedAssembly = parsed.ParsedAssemblies.FirstOrDefault().Value;

			var ver = Version.Parse(package.Version);
			ver = new Version(ver.Major, ver.Minor, ver.Build, 0);
			var path = $"data/api/{package.Name}/{ver}";

			Directory.CreateDirectory(path);
			foreach (var type in parsedAssembly.Types)
			{
				var res = await _viewRenderService.RenderToStringAsync("Api/ApiType", new ApiTypeViewModel
				{
					Type = type
				});

				await File.WriteAllTextAsync($"{path}/{type.FullName}.html", res);
			}

			{
				var res = await _viewRenderService.RenderToStringAsync("Api/ApiAssembly", new ApiAssemblyViewModel
				{
					Types = parsedAssembly.Types,
					Assembly = parsedAssembly.Assembly
				});

				await File.WriteAllTextAsync($"{path}/__sidebar.html", res);
			}
		}

		private string[] GetPackageIds()
		{
			// TODO: By settings

			return new []
			{
				"SampSharp.Entities"
			};
		}

		private async Task<IEnumerable<AvailablePackage>> DiscoverPackages()
		{
			var packages = new List<AvailablePackage>();

			foreach (var id in GetPackageIds())
			{
				var url = $@"http://nuget.timpotze.nl/api/v2/FindPackagesById()?id='{id}'&semVerLevel=2.0.0";

				await using var stream = await _httpClient.GetStreamAsync(url);

				var document = new XmlDocument();
				document.Load(stream);
				var nsManager = new XmlNamespaceManager(document.NameTable);
				nsManager.AddNamespace("a", "http://www.w3.org/2005/Atom");
				nsManager.AddNamespace("d", "http://schemas.microsoft.com/ado/2007/08/dataservices");
				nsManager.AddNamespace("m", "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata");

				foreach (XmlElement entry in document.DocumentElement.SelectNodes("//a:entry", nsManager))
				{
					var name = entry["title"].InnerText;
					var version = entry.SelectSingleNode("m:properties/d:Version", nsManager).InnerText;
					var src = entry["content"].GetAttribute("src");

					packages.Add(new AvailablePackage
					{
						Name = name,
						Version = version,
						DownloadUrl = src
					});
				}
			}

			return packages;
		}

		private class AvailablePackage
		{
			public string Name { get; set; }
			public string Version { get; set; }
			public string DownloadUrl { get; set; }
		}
	}
}
