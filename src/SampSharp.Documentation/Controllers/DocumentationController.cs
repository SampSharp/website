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

using Microsoft.AspNetCore.Mvc;
using SampSharp.Documentation.Models;
using SampSharp.Documentation.Repositories;

namespace SampSharp.Documentation.Controllers
{
	public class DocumentationController : SampSharpController
	{
		public DocumentationController(IVersionBuilder versionBuilder, IDataRepository dataRepository) :
			base(versionBuilder, dataRepository)
		{
		}

		[ResponseCache(Duration = 60 * 15, Location = ResponseCacheLocation.Any)]
		public IActionResult Index(string versionOrPage = null, string page = null)
		{
			SetCurrentPage(versionOrPage, page);

			if (Page == null) return NotFound();
			if (Page.Meta.RedirectUrl != null) return RedirectPermanent(Page.Meta.RedirectUrl);
			if (Page.Meta.RedirectPage != null)
			{
				return RedirectToRoutePermanent("documentation",
					new
					{
						versionOrPage = PageVersion.Tag,
						page = Page.Meta.RedirectPage
					});
			}

			return View(new DocumentationViewModel
			{
				Version = PageVersion,
				Title = Page.Meta.Title,
				Content = Page.Content,
				LastModification = Page.Meta.LastModification,
				Sidebar = Sidebar,
				VersionPicker = VersionPicker,
				EditUrl = Page.Meta.EditUrl
			});
		}
	}
}