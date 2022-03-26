using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SampSharp.DocX.Data;

namespace SampSharp.DocX.Debug
{
	class Program
	{
		static async Task Main(string[] args)
		{
			var imp = new ApiDocumentationImporter(new DocsDbContext());

			await imp.ImportAllFromNuGet();

			Console.ReadLine();
		}
	}
}
