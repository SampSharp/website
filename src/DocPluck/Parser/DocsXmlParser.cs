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

using System.Collections.Generic;
using System.Linq;
using System.Xml;
using DocPluck.XmlDocElement;
using DocPluck.XmlInfo;

namespace DocPluck.Parser
{
	internal class DocsXmlParser
	{
		private string ParseAssembly(XmlNode el)
		{
			return el["name"]?.InnerText;
		}

		private XmlDocListType ParseDocListType(string name)
		{
			return name switch
			{
				"number" => XmlDocListType.Number,
				"table" => XmlDocListType.Table,
				_ => XmlDocListType.Bullet
			};
		}

		private XmlDocListItem ParseListItem(XmlNode el)
		{
			return new XmlDocListItem(ParseText(el["term"]), ParseText(el["description"]));
		}

		private XmlDocList ParseList(XmlElement el)
		{
			XmlDocListItem header = null;
			var items = new List<XmlDocListItem>();
			foreach (XmlElement e in el.ChildNodes)
				switch (e.Name)
				{
					case "listheader":
						header = ParseListItem(e);
						break;
					case "item":
						items.Add(ParseListItem(e));
						break;
				}

			return new XmlDocList(ParseDocListType(el.GetAttribute("type")), header, items.ToArray());
		}

		private DocText ParseText(XmlNode el)
		{
			if (el == null)
				return null;
			var elements = new object[el.ChildNodes.Count];

			var index = 0;
			foreach (XmlNode node in el)
				switch (node)
				{
					case XmlText text:
						elements[index++] = text.Value.Trim();
						break;
					case XmlElement element:
					{
						elements[index++] = element.Name switch
						{
							"see" => (object) new XmlDocSee(ParseCref(element)),
							"para" => new XmlDocPara(ParseText(element)),
							"paramref" => new XmlDocParamReference(element.GetAttribute("name").Trim()),
							"typeparamref" => new XmlDocTypeParamReference(element.GetAttribute("name").Trim()),
							"c" => new DocSingleLineCode(element.InnerText.Trim()),
							"list" => ParseList(element),
							_ => new DocUnknownTextElement(element.OuterXml.Trim())
						};
						break;
					}
				}

			return new DocText(elements);
		}

		private CodeReference ParseCref(XmlElement el)
		{
			var cref = el.GetAttribute("cref").Trim();

			return string.IsNullOrEmpty(cref) ? null : CodeReference.Parse(cref);
		}

		private XmlDerivedInfo ParseMember(XmlElement el)
		{
			var result = new XmlDerivedInfo
			{
				Name = CodeReference.Parse(el.GetAttribute("name"))
			};

			foreach (XmlElement e in el.ChildNodes)
			{
				string name;
				CodeReference cref;
				switch (e.Name)
				{
					case "summary":
						result.Summary += ParseText(e);
						break;
					case "remarks":
						result.Remarks += ParseText(e);
						break;
					case "example":
						result.Example += ParseText(e);
						break;
					case "value":
						result.Value += ParseText(e);
						break;
					case "returns":
						result.Returns += ParseText(e);
						break;
					case "param":
						name = e.GetAttribute("name").Trim();
						result.Parameters.TryGetValue(name, out var previousParam);
						result.Parameters[name] = previousParam + ParseText(e);
						break;
					case "typeparam":
						name = e.GetAttribute("name").Trim();
						result.TypeParameters.TryGetValue(name, out var previousType);
						result.TypeParameters[name] = previousType + ParseText(e);
						break;
					case "permission":
						cref = ParseCref(e);
						result.Permissions.TryGetValue(cref, out var previousPermission);
						result.Permissions[cref] = previousPermission + ParseText(e);
						break;
					case "exception":
						cref = ParseCref(e);
						result.Exceptions.TryGetValue(cref, out var previousException);
						result.Exceptions[cref] = previousException + ParseText(e);
						break;
					case "inheritdoc":
						result.InheritDoc = true;
						result.InheritDocCref = ParseCref(e);
						break;
					case "seealso":
						result.SeeAlso.Add(ParseCref(e));
						break;
				}
			}

			return result;
		}

		private XmlDocBaseInfo[] CreateMemberRelations(IReadOnlyCollection<XmlDerivedInfo> members)
		{
			var namespacesDict = members
				.Where(m => m.Name.Kind == CodeReferenceKind.Namespace)
				.Select(m => new XmlDocNamespaceInfo(m))
				.ToDictionary(n => n.Name, n => n);

			var typesDict = members
				.Where(m => m.Name.Kind == CodeReferenceKind.Type)
				.Select(m => new XmlDocTypeInfo(m))
				.ToDictionary(t => t.Name, t => t);

			var eventsDict = members
				.Where(m => m.Name.Kind == CodeReferenceKind.Event)
				.Select(m => new XmlDocEventInfo(m))
				.ToList();

			var fieldsDict = members
				.Where(m => m.Name.Kind == CodeReferenceKind.Field)
				.Select(m => new XmlDocFieldInfo(m))
				.ToList();

			var propertiesDict = members
				.Where(m => m.Name.Kind == CodeReferenceKind.Property)
				.Select(m => new XmlDocPropertyInfo(m))
				.ToList();

			var methodsDict = members
				.Where(m => m.Name.Kind == CodeReferenceKind.Method)
				.Select(m => new XmlDocMethodInfo(m))
				.ToList();

			XmlDocTypeInfo Get(XmlDocBaseInfo m)
			{
				var key = $"{m.XmlDerivedInfo.Name.Namespace}.{m.XmlDerivedInfo.Name.TypeName}";
				if (!typesDict.TryGetValue(key, out var value))
					typesDict[key] = value = new XmlDocTypeInfo(new XmlDerivedInfo
					{
						Name = new CodeReference(CodeReferenceKind.Type, m.XmlDerivedInfo.Name.Namespace, m.XmlDerivedInfo.Name.TypeName, null)
					});

				return value;
			}

			// Add relations
			foreach (var m in methodsDict)
			{
				var value = Get(m);
				value.Methods.Add(m);
				m.DeclaringType = value;
			}

			foreach (var m in propertiesDict)
			{
				var value = Get(m);
				value.Properties.Add(m);
				m.DeclaringType = value;
			}

			foreach (var m in fieldsDict)
			{
				var value = Get(m);
				value.Fields.Add(m);
				m.DeclaringType = value;
			}

			foreach (var m in eventsDict)
			{
				var value = Get(m);
				value.Events.Add(m);
				m.DeclaringType = value;
			}

			foreach (var (_, m) in typesDict)
			{
				var key = m.XmlDerivedInfo.Name.Namespace;
				if (!namespacesDict.TryGetValue(key, out var value))
					namespacesDict[key] = value = new XmlDocNamespaceInfo(new XmlDerivedInfo
					{
						Name = new CodeReference(CodeReferenceKind.Type, m.XmlDerivedInfo.Name.Namespace, null, null)
					});

				value.Types.Add(m);
				m.Namespace = value;
			}

			return namespacesDict.Values
				.OfType<XmlDocBaseInfo>()
				.Concat(typesDict.Values)
				.Concat(eventsDict)
				.Concat(fieldsDict)
				.Concat(propertiesDict)
				.Concat(methodsDict)
				.ToArray();
		}

		private XmlDocBaseInfo[] ParseMembers(XmlNode el)
		{
			var members = el.ChildNodes
				.OfType<XmlElement>()
				.Select(ParseMember)
				.ToList();

			return CreateMemberRelations(members);
		}

		public XmlDocumentation Parse(XmlElement document)
		{
			var assembly = document["assembly"];
			var members = document["members"];

			var asm = assembly == null ? null : ParseAssembly(assembly);
			var mem = members == null ? null : ParseMembers(members);

			return new XmlDocumentation
			{
				AssemblyName = asm,
				Members = mem
			};
		}
	}
}