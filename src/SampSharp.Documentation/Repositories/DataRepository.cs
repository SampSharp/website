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
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SampSharp.Documentation.Configuration;
using SampSharp.Documentation.Models;

namespace SampSharp.Documentation.Repositories
{
	public class DataRepository : IDataRepository
	{
		private readonly IOptions<StorageOptions> _options;

		public DataRepository(IOptions<StorageOptions> options)
		{
			_options = options;
		}

		private string DataPath
		{
			get
			{
				var p = _options.Value.DataPath ?? "data";

				if (!Path.IsPathRooted(p))
					p = Path.Combine(Directory.GetCurrentDirectory(), p);

				Directory.CreateDirectory(p);

				return p;
			}
		}

		public bool IsEmpty => !Directory.GetFiles(DataPath).Any();

		private Stream Read(string path)
		{
			if (path == null) throw new ArgumentNullException(nameof(path));

            Debug.WriteLine($"start read {path}");

			path = AbsolutePath(path) ?? throw new ArgumentException("Invalid path", nameof(path));

			if (!File.Exists(path)) throw new FileNotFoundException();

			return new FileStream(path, FileMode.Open);
		}

		private string AbsolutePath(string path)
		{
			if (string.IsNullOrWhiteSpace(path))
				return null;

			var dataPath = DataPath;
			path = Path.Combine(dataPath, path.ToLower());

			path = Path.GetFullPath(path);

			return !path.StartsWith(dataPath) ? null : path;
		}

		private static string BranchNameToPathString(string branch)
		{
			return branch?.Replace("/", "---").Replace("\\", "-----").ToLower();
		}

		private string AbsolutePath(string branch, string path)
		{
			if (string.IsNullOrWhiteSpace(branch) || string.IsNullOrWhiteSpace(path))
				return null;

			var dataPath = DataPath;
			var branchPath = Path.Combine(dataPath, BranchNameToPathString(branch));
			branchPath = Path.GetFullPath(branchPath);

			if (!branchPath.StartsWith(dataPath))
				return null;

			path = Path.Combine(branchPath, path.ToLower());
			path = Path.GetFullPath(path);

			return !path.StartsWith(branchPath) ? null : path;
		}

		public bool DocExists(string branch, string path)
		{
			if (branch == null) throw new ArgumentNullException(nameof(branch));
			if (path == null) throw new ArgumentNullException(nameof(path));

			path = AbsolutePath(branch, path) ?? throw new ArgumentException("Invalid path", nameof(path));

			var htmlPath = Path.Combine(DataPath, path + ".html");
			var metaPath = Path.Combine(DataPath, path + ".json");

			return File.Exists(htmlPath) && File.Exists(metaPath);
		}
		
        private object _configLock = new object();
        private DocConfiguration _config;
		public DocConfiguration GetDocConfiguration()
		{
            if (_config != null)
            {
                return _config;
            }

            lock (_configLock)
            {
                if (_config != null)
                {
                    return _config;
                }

                var serializer = new JsonSerializer();

                using (var fileStream = Read("config.json"))
                using (var streamReader = new StreamReader(fileStream))
                using (var jsonTextReader = new JsonTextReader(streamReader))
                {
					var config = serializer.Deserialize<DocConfiguration>(jsonTextReader);
                    _config = config;
                    return config;
                }
            }
        }

		public void StoreDocConfiguration(DocConfiguration config)
		{
			if (config == null) throw new ArgumentNullException(nameof(config));

            lock (_configLock)
            {
                var json = JsonConvert.SerializeObject(config);

                File.WriteAllText(AbsolutePath("config.json"), json);
                _config = config;
            }
        }

		public DocFile GetDocFile(string branch, string path)
		{
			if (branch == null) throw new ArgumentNullException(nameof(branch));
			if (path == null) throw new ArgumentNullException(nameof(path));

			var htmlPath = AbsolutePath(branch, path.ToLower() + ".html");
			var metaPath = AbsolutePath(branch, path.ToLower() + ".json");

			if (htmlPath == null || metaPath == null || !File.Exists(htmlPath) || !File.Exists(metaPath)) return null;

			var serializer = new JsonSerializer();

			using (var fileStream = File.OpenRead(metaPath))
			using (var streamReader = new StreamReader(fileStream))
			using (var jsonTextReader = new JsonTextReader(streamReader))
			{
				var meta = serializer.Deserialize<DocMeta>(jsonTextReader);
				var content = File.ReadAllText(htmlPath);

				return new DocFile
				{
					Content = content,
					Meta = meta
				};
			}
		}

		public void StoreDocFile(string branch, string path, DocFile file)
		{
			if (branch == null) throw new ArgumentNullException(nameof(branch));
			if (path == null) throw new ArgumentNullException(nameof(path));
			if (file == null) throw new ArgumentNullException(nameof(file));

			var htmlPath = AbsolutePath(branch, path.ToLower() + ".html");
			var metaPath = AbsolutePath(branch, path.ToLower() + ".json");

			if (File.Exists(htmlPath) || File.Exists(metaPath)) throw new Exception("File already exists");

			var directory = Path.GetDirectoryName(htmlPath);
			Directory.CreateDirectory(directory);

			var metaString = JsonConvert.SerializeObject(file.Meta);

			File.WriteAllText(htmlPath, file.Content);
			File.WriteAllText(metaPath, metaString);
		}
		
		public (Stream, string) GetAsset(string branch, string path)
		{
			if (branch == null) throw new ArgumentNullException(nameof(branch));
			if (path == null) throw new ArgumentNullException(nameof(path));

			path = AbsolutePath(branch, path.ToLower());

			if (path == null || !File.Exists(path) || !File.Exists(path + ".asset")) return (null, null);

			var fileStream = File.OpenRead(path);

			new FileExtensionContentTypeProvider().TryGetContentType(path, out var contentType);

			return (fileStream, contentType ?? "application/octet-stream");
		}

		public void StoreAsset(string branch, string path, Stream assetStream)
		{
			if (branch == null) throw new ArgumentNullException(nameof(branch));
			if (path == null) throw new ArgumentNullException(nameof(path));
			if (assetStream == null) throw new ArgumentNullException(nameof(assetStream));

			path = AbsolutePath(branch, path.ToLower());

			if (File.Exists(path)) throw new Exception("File already exists");

			var directory = Path.GetDirectoryName(path);
			Directory.CreateDirectory(directory);

			using (var fs = File.OpenWrite(path))
			{
				assetStream.CopyTo(fs);
				fs.Flush();
			}

			File.WriteAllText(path + ".asset", "");
		}

		public void DeleteBranch(string branch)
		{
			if (branch == null) throw new ArgumentNullException(nameof(branch));

			var path = AbsolutePath(BranchNameToPathString(branch)) ?? throw new ArgumentException("Invalid branch", nameof(branch));

			if (Directory.Exists(path)) Directory.Delete(path, true);
		}
	}
}