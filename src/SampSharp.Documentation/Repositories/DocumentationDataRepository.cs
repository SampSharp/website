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
using System.IO;
using Microsoft.AspNetCore.StaticFiles;
using SampSharp.Documentation.Models;
using SampSharp.Documentation.Services;

namespace SampSharp.Documentation.Repositories
{
	public class DocumentationDataRepository : IDocumentationDataRepository
	{
		private readonly IStorageService _storageService;

		public DocumentationDataRepository(IStorageService storageService)
		{
			_storageService = storageService;
		}
		
		public bool DocExists(string branch, string path)
		{
			if (branch == null) throw new ArgumentNullException(nameof(branch));
			if (path == null) throw new ArgumentNullException(nameof(path));

			return _storageService.ObjectExists(StorageKey.ForDocumentationData(branch, path));
		}

		public DocConfiguration GetDocConfiguration()
		{
			return _storageService.ReadObject<DocConfiguration>(StorageKey.ForDocumentationData("__config", "config"));
		}

		public void StoreDocConfiguration(DocConfiguration config)
		{
			if (config == null) throw new ArgumentNullException(nameof(config));

			_storageService.StoreObject(StorageKey.ForDocumentationData("__config", "config"), config);
		}

		public DocFile GetDocFile(string branch, string path)
		{
			if (branch == null) throw new ArgumentNullException(nameof(branch));
			if (path == null) throw new ArgumentNullException(nameof(path));

			return _storageService.ReadObject<DocFile>(StorageKey.ForDocumentationData(branch, path.ToLowerInvariant()));
		}

		public void StoreDocFile(string branch, string path, DocFile file)
		{
			if (branch == null) throw new ArgumentNullException(nameof(branch));
			if (path == null) throw new ArgumentNullException(nameof(path));
			if (file == null) throw new ArgumentNullException(nameof(file));

			_storageService.StoreObject<DocFile>(StorageKey.ForDocumentationData(branch, path.ToLowerInvariant()), file);
		}
		
		public (Stream, string) GetAsset(string branch, string path)
		{
			if (branch == null) throw new ArgumentNullException(nameof(branch));
			if (path == null) throw new ArgumentNullException(nameof(path));

			if (!_storageService.ObjectExists(StorageKey.ForDocumentationAssetMeta(branch, path.ToLowerInvariant())))
				return (null, null);

			var fileStream = _storageService.Open(StorageKey.ForDocumentationAsset(branch, path.ToLowerInvariant()));

			new FileExtensionContentTypeProvider().TryGetContentType(path, out var contentType);

			return (fileStream, contentType ?? "application/octet-stream");
		}

		public void StoreAsset(string branch, string path, Stream assetStream)
		{
			if (branch == null) throw new ArgumentNullException(nameof(branch));
			if (path == null) throw new ArgumentNullException(nameof(path));
			if (assetStream == null) throw new ArgumentNullException(nameof(assetStream));

			_storageService.Store(StorageKey.ForDocumentationAsset(branch, path.ToLowerInvariant()), assetStream);
			_storageService.StoreObject(StorageKey.ForDocumentationAssetMeta(branch, path.ToLowerInvariant()), new object());
		}

		public void DeleteBranch(string branch)
		{
			if (branch == null) throw new ArgumentNullException(nameof(branch));

			_storageService.DeleteSet(StorageKey.ForDocumentationVersion(branch));
		}
	}
}