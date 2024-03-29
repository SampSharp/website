﻿// SampSharp.Documentation
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

using SampSharp.Documentation.Models;
using SampSharp.Documentation.Repositories;

namespace SampSharp.Documentation
{
	public class VersionBuilder : IVersionBuilder
	{
		private readonly IDataRepository _docVersionRepository;

		public VersionBuilder(IDataRepository docVersionRepository)
		{
			_docVersionRepository = docVersionRepository;
		}

		public VersionViewModel[] GetAll()
		{
			var def = _docVersionRepository.GetDocConfiguration();

			return def.Versions
				.Select(v => new VersionViewModel
				{
					DisplayName = v.DisplayName,
					Tag = v.Tag,
					IsDefault = v == def.DefaultVersion,
					DefaultPage = v.DefaultPage?.ToLower()
				}).ToArray();
		}
	}
}