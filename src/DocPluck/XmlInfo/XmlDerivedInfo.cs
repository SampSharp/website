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
using System.Runtime.Serialization;
using DocPluck.XmlDocElement;

namespace DocPluck.XmlInfo
{
	[DataContract]
	public class XmlDerivedInfo
	{
		[DataMember]
		public bool InheritDoc { get; set; }
		[DataMember]
		public CodeReference InheritDocCref { get; set; }
		[DataMember]
		public CodeReference Name { get; set; }
		[DataMember]
		public DocText Summary { get; set; }
		[DataMember]
		public DocText Remarks { get; set; }
		[DataMember]
		public DocText Example { get; set; }
		[DataMember]
		public DocText Value { get; set; }
		[DataMember]
		public DocText Returns { get; set; }
		[DataMember]
		public Dictionary<string, DocText> Parameters { get; } = new Dictionary<string, DocText>();
		[DataMember]
		public Dictionary<string, DocText> TypeParameters { get; } = new Dictionary<string, DocText>();
		[DataMember]
		public Dictionary<CodeReference, DocText> Exceptions { get; } = new Dictionary<CodeReference, DocText>();
		[DataMember]
		public Dictionary<CodeReference, DocText> Permissions { get; } = new Dictionary<CodeReference, DocText>();
		[DataMember]
		public List<CodeReference> SeeAlso { get; } = new List<CodeReference>();
	}
}