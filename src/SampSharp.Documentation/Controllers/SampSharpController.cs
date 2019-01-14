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
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using SampSharp.Documentation.Models;
using SampSharp.Documentation.Repositories;

namespace SampSharp.Documentation.Controllers
{
	public class SampSharpController : Controller
	{
		protected const string SidebarName = "_Sidebar";
		private readonly IDataRepository _dataRepository;
		private readonly IVersionBuilder _versionBuilder;
		private VersionViewModel[] _versions;

		public SampSharpController(IVersionBuilder versionBuilder, IDataRepository dataRepository)
		{
			_versionBuilder = versionBuilder;
			_dataRepository = dataRepository;
		}

		protected VersionViewModel PageVersion { get; private set; }
		protected VersionViewModel FallbackVersion { get; private set; }

		protected VersionViewModel[] Versions => _versions ?? (_versions = _versionBuilder.GetAll());
		public DocFile Page { get; private set; }

		protected SidebarViewModel Sidebar => new SidebarViewModel
		{
			Content = GetFile(FallbackVersion?.Tag, SidebarName)?.Content
		};

		protected VersionPickerViewModel VersionPicker => new VersionPickerViewModel
		{
			Current = FallbackVersion,
			Versions = Versions
		};

		// TODO: Add 503 Service Unavailable when no cfg can be found

		protected void SetCurrentPage(string versionOrPage, string page)
		{
			VersionViewModel version;
			versionOrPage = versionOrPage?.ToLower();
			page = page?.ToLower();

			if (page != null && versionOrPage == null) throw new ArgumentException("versionOrPage cannot be null when page is not null");

			// Determine whether versionOrPage is version or page
			var versions = Versions;
			var defaultVersion = versions.First(v => v.IsDefault);

			// If no page is specified, versionOrPage is either the specified page on the
			// default version, or the default page on the specified version.
			if (page == null)
			{
				var file = GetFile(defaultVersion.Tag, versionOrPage);

				if (file != null)
				{
					// versionOrPage is a page.
					PageVersion = defaultVersion;
					FallbackVersion = defaultVersion;
					Page = file;
				}
				else
				{
					// versionOrPage must be a version.
					version = versions.FirstOrDefault(v => v.Tag == versionOrPage);

					PageVersion = version;
					FallbackVersion = version ?? defaultVersion;
					Page = version != null
						? GetFile(version.Tag, version.DefaultPage)
						: null;
				}

				return;
			}

			// versionOrPage must be a version.
			version = versions.FirstOrDefault(v => v.Tag == versionOrPage);

			if (version == null)
			{
				FallbackVersion = defaultVersion;
				return;
			}

			Page = GetFile(version.Tag, page);
			PageVersion = version;
			FallbackVersion = version;
		}

		protected DocFile GetFile(string version, string name)
		{
			version = version?.ToLower();
			name = name?.ToLower();

			return version != null && name != null
				? _dataRepository.GetDocFile(version, name)
				: null;
		}
	}
}