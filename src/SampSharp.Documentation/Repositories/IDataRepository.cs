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

namespace SampSharp.Documentation.Repositories
{
	public interface IDataRepository
	{
		bool IsEmpty { get; }

		void DeleteBranch(string branch);
		bool DocExists(string branch, string path);
		DocConfiguration GetDocConfiguration();
		void StoreDocConfiguration(DocConfiguration config);
		DocFile GetDocFile(string branch, string path);
		void StoreDocFile(string branch, string path, DocFile file);
		(Stream, string) GetAsset(string branch, string path);
		void StoreAsset(string branch, string path, Stream assetStream);
	}
}