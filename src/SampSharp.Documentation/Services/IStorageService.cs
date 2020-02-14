using System.IO;

namespace SampSharp.Documentation.Services
{
	public interface IStorageService
	{
		void DeleteSet(StorageKey key);
		T ReadObject<T>(StorageKey key);
		void StoreObject<T>(StorageKey key, T value);
		Stream Open(StorageKey key);
		bool ObjectExists(StorageKey key);
		void Store(StorageKey key, Stream stream);
	}
}