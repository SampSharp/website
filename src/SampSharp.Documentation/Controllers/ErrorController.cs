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

using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SampSharp.Documentation.Models;
using SampSharp.Documentation.Repositories;

namespace SampSharp.Documentation.Controllers
{
	public class ErrorController : SampSharpController
	{
		public ErrorController(IVersionBuilder versionBuilder, IDataRepository dataRepository) :
			base(versionBuilder, dataRepository)
		{
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult ServerError()
		{
			return View(new ErrorViewModel
			{
				RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
			});
		}

		[ResponseCache(Duration = 60 * 15, Location = ResponseCacheLocation.Any)]
		public IActionResult PageNotFound()
		{
			SetCurrentPage(Versions.First(v => v.IsDefault).Tag, SidebarName); // TODO: Actual current version
			return View(new ErrorViewModel
			{
				RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
				Sidebar = Sidebar
			});
		}
	}
}