using System.IO;

namespace SampSharp.Documentation.Services
{
	public interface IFileSystemService
	{
		string ReadAllText(string path);
		void WriteAllText(string path, string text);
		void DeleteDirectory(string path);
		Stream OpenRead(string path);
		Stream OpenWrite(string path);
		bool FileExists(string path);
	}
}