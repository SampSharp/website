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

using System.Runtime.Serialization;
using DocPluck.XmlInfo;

namespace DocPluck.Reflection
{
	/// <summary>
	/// Provides access to namespace metadata.
	/// </summary>
	[DataContract]
	public class DocsNamespaceInfo
	{
		/// <summary>
		/// Gets the types of this namespace.
		/// </summary>
		[DataMember]
		public DocsTypeInfo[] Types { get; set; }

		/// <summary>
		/// Gets the exported type of this namespaces.
		/// </summary>
		[DataMember]
		public DocsTypeInfo[] ExportedTypes { get; set; }

		/// <summary>
		/// Gets the namespaces of this namespace.
		/// </summary>
		[DataMember]
		public DocsNamespaceInfo[] Namespaces { get; set; }

		/// <summary>
		/// Gets the parent of this namespace.
		/// </summary>
		[DataMember]
		public DocsNamespaceInfo Parent { get; set; }

		/// <summary>
		/// Gets the name of this namespace.
		/// </summary>
		[DataMember]
		public string Name { get; set; }

		/// <summary>
		/// Gets the documentation of this namespace.
		/// </summary>
		[DataMember]
		public XmlDocNamespaceInfo Documentation { get; set; }
	}
}