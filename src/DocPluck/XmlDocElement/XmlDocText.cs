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
using System.Runtime.Serialization;

namespace DocPluck.XmlDocElement
{
	public class DocText : XmlDocElement
	{
		public DocText(object[] elements)
		{
			Elements = elements ?? throw new ArgumentNullException(nameof(elements));
		}
		
		[DataMember]
		public object[] Elements { get; }

		public override string ToString()
		{
			return string.Join(" ", Elements);
		}

		public DocText Concat(DocText other)
		{
			if (other == null)
				return this;

			var el = new object[Elements.Length + other.Elements.Length];

			Array.Copy(Elements, el, Elements.Length);
			Array.Copy(other.Elements, 0, el, Elements.Length, other.Elements.Length);
			return new DocText(el);
		}

		public static DocText operator +(DocText lhs, DocText rhs)
		{
			return lhs == null
				? rhs
				: rhs == null
					? lhs
					: lhs.Concat(rhs);
		}
	}
}