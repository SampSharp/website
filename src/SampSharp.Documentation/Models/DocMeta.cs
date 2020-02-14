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

namespace SampSharp.Documentation.Models
{
	public class DocMeta
	{
		public string Title { get; set; }
		public string RedirectUrl { get; set; }
		public string RedirectPage { get; set; }
		public DateTime LastModification { get; set; }
		public string EditUrl { get; set; }
		public string Introduction { get; set; }
		public List<ArticleQuickLink> QuickLinks { get; set; }
	}
}