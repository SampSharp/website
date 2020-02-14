using System;
using System.IO;
using Microsoft.Extensions.Options;
using SampSharp.Documentation.Configuration;

namespace SampSharp.Documentation.Services
{
	public class FileSystemService : IFileSystemService
	{
		private readonly IOptions<StorageOptions> _options;

		public FileSystemService(IOptions<StorageOptions> options)
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
		
		private string AbsolutePath(string path)
		{
			if (string.IsNullOrWhiteSpace(path))
				return null;

			var dataPath = DataPath;
			path = Path.Combine(dataPath, path.ToLower());

			path = Path.GetFullPath(path);

			return !path.StartsWith(dataPath) ? null : path;
		}

		public string ReadAllText(string path)
		{
			if (path == null) throw new ArgumentNullException(nameof(path));

			path = AbsolutePath(path);

			return File.ReadAllText(path);
		}
		
		public void WriteAllText(string path, string text)
		{
			if (path == null) throw new ArgumentNullException(nameof(path));
			
			path = AbsolutePath(path);

			Directory.CreateDirectory(Path.GetDirectoryName(path));

			File.WriteAllText(path, text);
		}

		public void DeleteDirectory(string path)
		{
			if (path == null) throw new ArgumentNullException(nameof(path));
			
			path = AbsolutePath(path);

			if (Directory.Exists(path))
				Directory.Delete(path, true);
		}

		public Stream OpenRead(string path)
		{
			if (path == null) throw new ArgumentNullException(nameof(path));

			path = AbsolutePath(path);

			return File.OpenRead(path);
		}

		public Stream OpenWrite(string path)
		{
			if (path == null) throw new ArgumentNullException(nameof(path));

			path = AbsolutePath(path);
			
			Directory.CreateDirectory(Path.GetDirectoryName(path));

			if (File.Exists(path))
				File.Delete(path);

			return File.OpenWrite(path);
		}

		public bool FileExists(string path)
		{
			if (path == null) throw new ArgumentNullException(nameof(path));

			path = AbsolutePath(path);

			return File.Exists(path);
		}
	}
}