using System;
using System.IO;
using Newtonsoft.Json;

namespace SampSharp.Documentation.Services
{
	public class StorageService : IStorageService
	{
		private readonly IFileSystemService _fileSystemService;

		public StorageService(IFileSystemService fileSystemService)
		{
			_fileSystemService = fileSystemService;
		}

		public void StoreObject<T>(StorageKey key, T value)
		{
			if (key == null) throw new ArgumentNullException(nameof(key));
			if (value == null) throw new ArgumentNullException(nameof(value));
			_fileSystemService.WriteAllText(key.KeyPath, JsonConvert.SerializeObject(value));
		}

		public void DeleteSet(StorageKey key)
		{
			if (key == null) throw new ArgumentNullException(nameof(key));
			_fileSystemService.DeleteDirectory(key.KeyPath);
		}

		public T ReadObject<T>(StorageKey key)
		{
			if (key == null) throw new ArgumentNullException(nameof(key));

			if (!_fileSystemService.FileExists(key.KeyPath))
				return default;

			var txt = _fileSystemService.ReadAllText(key.KeyPath);

			return JsonConvert.DeserializeObject<T>(txt);
		}

		public bool ObjectExists(StorageKey key)
		{
			if (key == null) throw new ArgumentNullException(nameof(key));
			return _fileSystemService.FileExists(key.KeyPath);
		}

		public void Store(StorageKey key, Stream stream)
		{
			if (key == null) throw new ArgumentNullException(nameof(key));
			if (stream == null) throw new ArgumentNullException(nameof(stream));

			using var fileStream = _fileSystemService.OpenWrite(key.KeyPath);
			stream.CopyTo(fileStream);
		}

		public Stream Open(StorageKey key)
		{
			if (key == null) throw new ArgumentNullException(nameof(key));
			
			if (!_fileSystemService.FileExists(key.KeyPath))
				return null;

			return _fileSystemService.OpenRead(key.KeyPath);
		}
	}
}
