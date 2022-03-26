// SampSharp.Documentation
// Copyright 2020 Tim Potze
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
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using SampSharp.Documentation.Models;
using SampSharp.Documentation.Repositories;
using SampSharp.Documentation.Services;

namespace SampSharp.Documentation.Controllers
{
	public class DocumentationController : Controller
	{
		private readonly IDocumentationDataRepository _documentationDataRepository;
		private readonly IDocsVersionBuilder _versionBuilder;

		public DocumentationController(IDocsVersionBuilder versionBuilder, IDocumentationDataRepository documentationDataRepository)
		{
			_versionBuilder = versionBuilder;
			_documentationDataRepository = documentationDataRepository;
		}

		[ResponseCache(Duration = 60 * 15, Location = ResponseCacheLocation.Any)]
		public IActionResult Index(string version = null, string page = null)
		{
			// Legacy redirect
			if (page == null)
			{
				var defaultVersion = _documentationDataRepository.GetDocConfiguration().DefaultVersion;

				page = version ?? defaultVersion.DefaultPage;
				version = defaultVersion.Tag;

				return RedirectToRoutePermanent("docs",
					new
					{
						versionOrPage = version,
						page
					});
			}

			// Collect info
			if (version == null) throw new ArgumentNullException(nameof(version));

			var versions = _versionBuilder.GetAll();
			var versionObj = versions.FirstOrDefault(v => v.Tag == version);

			if (versionObj == null) return NotFound();

			var lowerVersion = version.ToLower();
			var lowerName = page.ToLower();

			var docPage = _documentationDataRepository.GetDocFile(lowerVersion, lowerName);

			// Asset
			if (docPage == null)
			{
				var (str, mime) = _documentationDataRepository.GetAsset(versionObj.Tag, page);

				return str != null && mime != null
					? (IActionResult) File(str, mime)
					: NotFound();
			}

			// Redirects
			if (docPage.Meta.RedirectUrl != null)
				return RedirectPermanent(docPage.Meta.RedirectUrl);
			if (docPage.Meta.RedirectPage != null)
				return RedirectToRoutePermanent("docs",
					new
					{
						versionOrPage = versionObj.Tag,
						page = docPage.Meta.RedirectPage
					});

			// Actual documentation
			return View(new DocumentationViewModel
			{
				Title = docPage.Meta.Title,
				Content = docPage.Content,
				LastModification = docPage.Meta.LastModification,
				EditUrl = docPage.Meta.EditUrl,
				Introduction = docPage.Meta.Introduction,
				QuickLinks = docPage.Meta.QuickLinks,
				Menu = versionObj.Menu
			});
		}
	}
}