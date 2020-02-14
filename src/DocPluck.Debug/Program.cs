using System;
using System.Linq;
using DocPluck.Parser;
using DocPluck.XmlInfo;

namespace DocPluck.Debug
{
	public class Program
	{
		enum X : byte{A}
		static void Main(string[] args)
		{
			var parser = new DocsParser();
//			var parsed = parser.ParseFromNugetPackage(@"D:\projects\sampsharp-website\src\SimpleSample\bin\Release\netstandard2.0\publish\SimpleSample.1.0.0.nupkg");
			var parsed = parser.ParseFromNugetPackage(@"C:\Users\Tim\Downloads\SampSharp.Entities.0.9.1.nupkg");

			foreach (var a in parsed.XmlDocumentation.Values)
			{
				foreach (var t in a.Members.OfType<XmlDocTypeInfo>())
				{
					Console.WriteLine(t.Name);

					foreach(var m in t.Properties)
						Console.WriteLine($"  {m.Name}");
				}
			}

			Console.WriteLine();
			Console.WriteLine("=====================================");
			Console.WriteLine();

			foreach (var asm in parsed.ParsedAssemblies)
			{
				TypeDebugPrinter.PrintTypes(asm.Value.Types);
			}
		}
	}
}
