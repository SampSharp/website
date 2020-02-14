using System;

namespace SampSharp.Documentation.Services
{
	public class StorageKey
	{
		public string KeyPath { get; }

		private StorageKey(string keyPath)
		{
			KeyPath = keyPath ?? throw new ArgumentNullException(nameof(keyPath));
		}
		
		private static string BranchNameToPathString(string branch)
		{
			return branch?.Replace("/", "---").Replace("\\", "-----").ToLower();
		}
		
		public static StorageKey ForDocumentationData(string version, string name)
		{
			if (version == null) throw new ArgumentNullException(nameof(version));
			if (name == null) throw new ArgumentNullException(nameof(name));

			return new StorageKey($"docs/{BranchNameToPathString(version)}/{name}.json");
		}
		
		public static StorageKey ForDocumentationAsset(string version, string name)
		{
			if (version == null) throw new ArgumentNullException(nameof(version));
			if (name == null) throw new ArgumentNullException(nameof(name));

			return new StorageKey($"docs/{BranchNameToPathString(version)}/{name}");
		}

		public static StorageKey ForDocumentationAssetMeta(string version, string name)
		{
			if (version == null) throw new ArgumentNullException(nameof(version));
			if (name == null) throw new ArgumentNullException(nameof(name));

			return new StorageKey($"docs/{BranchNameToPathString(version)}/{name}.asset");
		}

		public static StorageKey ForDocumentationVersion(string version)
		{
			if (version == null) throw new ArgumentNullException(nameof(version));

			return new StorageKey($"docs/{version}");
		}
	}
}