// DocPluck
// Copyright 2020 Tim Potze
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
using System.IO.Compression;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Xml;

namespace DocPluck.Parser
{
	public class DocsParser
	{
		private readonly DocsXmlParser _xmlParser = new DocsXmlParser();

		public ParsedAssembly ParseFromPE(string path)
		{
			if (path == null) throw new ArgumentNullException(nameof(path));

			if (!File.Exists(path))
				throw new FileNotFoundException("The file could not be found at the specified location.", path);

			return ParseFromPE(File.OpenRead(path));
		}

		public ParsedAssembly ParseFromPE(Stream stream)
		{
			if (stream == null) throw new ArgumentNullException(nameof(stream));

			if (!(stream is MemoryStream))
			{
				var m = new MemoryStream();
				stream.CopyTo(m);
				stream = m;
				stream.Seek(0, SeekOrigin.Begin);
			}

			using var peFile = new PEReader(stream);

			var reader = peFile.GetMetadataReader();
			var parser = new DocsPEParser(reader);

			return parser.Parse();
		}

		public XmlDocumentation ParseFromDocumentation(Stream stream)
		{
			if (stream == null) throw new ArgumentNullException(nameof(stream));
			var doc = new XmlDocument();
			doc.Load(stream);

			var root = doc.DocumentElement;

			var result = _xmlParser.Parse(root);

			return result;
		}

		public XmlDocumentation ParseFromDocumentation(string path)
		{
			if (path == null) throw new ArgumentNullException(nameof(path));

			if (!File.Exists(path))
				throw new FileNotFoundException("The file could not be found at the specified location.", path);

			return ParseFromDocumentation(File.OpenRead(path));
		}

		public NugetDocumentationSet ParseFromNugetPackage(Stream stream)
		{
			if (stream == null) throw new ArgumentNullException(nameof(stream));
			var result = new NugetDocumentationSet();
			using (var archive = new ZipArchive(stream))
			{
				foreach (var entry in archive.Entries)
				{
					var extension = Path.GetExtension(entry.Name).ToLowerInvariant();
					var directory = Path.GetDirectoryName(entry.FullName);

					if (directory == null)
						continue;

					var path = directory.Split(Path.DirectorySeparatorChar);

					if (path.Length != 2 || path[0] != "lib")
						continue;

					var targetFramework = path[1];

					switch (extension)
					{
						case ".xml" when entry.Name != "[Content_Types].xml":
						{
							using var entryStream = entry.Open();
							var docs = ParseFromDocumentation(entryStream);
							result.XmlDocumentation.Add(new AssemblyReference(targetFramework, docs.AssemblyName), docs);
							break;
						}
						case ".dll":
						{
							using var entryStream = entry.Open();
							var parsed = ParseFromPE(entryStream);
							result.ParsedAssemblies[parsed.Assembly.Name.Name] = parsed;
							break;
						}
					}
				}
			}

			foreach (var parsedAssembly in result.ParsedAssemblies.Values)
			{
				var (_, value) = result.XmlDocumentation.FirstOrDefault(kv => kv.Key.AssemblyName == parsedAssembly.Assembly.Name.Name);

				if (value != null)
					ApplyXmlDocumentation(value, parsedAssembly);
			}

			return result;
		}

		public NugetDocumentationSet ParseFromNugetPackage(string path)
		{
			if (path == null) throw new ArgumentNullException(nameof(path));

			if (!File.Exists(path))
				throw new FileNotFoundException("The file could not be found at the specified location.", path);

			return ParseFromNugetPackage(File.OpenRead(path));
		}

		public void ApplyXmlDocumentation(XmlDocumentation documentation, ParsedAssembly assembly)
		{
			DocumentationCombiner.Combine(documentation, assembly);
		}
	}

	internal static class DocumentationCombiner
	{
		public static void Combine(XmlDocumentation documentation, ParsedAssembly assembly)
		{
			foreach (var ns in assembly.Namespaces)
			{
				ns.Documentation = documentation.Get(ns);
			}

			foreach (var type in assembly.Types)
			{
				type.Documentation = documentation.Get(type);

				if (type.Documentation == null)
					continue;
				
				foreach (var method in type.Methods) method.Documentation = type.Documentation.Get(method);
				foreach (var method in type.Constructors) method.Documentation = type.Documentation.Get(method);
				foreach (var property in type.Properties) property.Documentation = type.Documentation.Get(property);
				foreach (var @event in type.Events) @event.Documentation = type.Documentation.Get(@event);
				foreach (var field in type.Fields) field.Documentation = type.Documentation.Get(field);
			}
		}
	}
}