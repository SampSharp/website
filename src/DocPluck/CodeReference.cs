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

namespace DocPluck
{
	/// <summary>
	/// Represents a reference to a code element, often referred to as "cref".
	/// </summary>
	[DataContract]
	public class CodeReference
	{
		public CodeReference(CodeReferenceKind kind, string ns, string typeName, string memberName)
		{
			Kind = kind;
			Namespace = ns;
			TypeName = typeName;
			MemberName = memberName;
		}
		
		[DataMember]
		public CodeReferenceKind Kind { get; }
		[DataMember]
		public string Namespace { get; }
		[DataMember]
		public string TypeName { get; }
		[DataMember]
		public string MemberName { get; }

		private char KindChar =>
			Kind switch
			{
				CodeReferenceKind.Namespace => 'N',
				CodeReferenceKind.Type => 'T',
				CodeReferenceKind.Field => 'F',
				CodeReferenceKind.Property => 'P',
				CodeReferenceKind.Method => 'M',
				CodeReferenceKind.Event => 'E',
				CodeReferenceKind.ErrorString => '!',
				_ => '?'
			};

		public static CodeReference Parse(string input)
		{
			if (input == null) throw new ArgumentNullException(nameof(input));
			if (input.Length < 3 || input[1] != ':')
				throw new ArgumentException("Invalid input", nameof(input));

			string ns;
			string type;
			string memberName = null;

			var kind = input[0] switch
			{
				'N' => CodeReferenceKind.Namespace,
				'T' => CodeReferenceKind.Type,
				'F' => CodeReferenceKind.Field,
				'M' => CodeReferenceKind.Method,
				'E' => CodeReferenceKind.Event,
				'P' => CodeReferenceKind.Property,
				'!' => CodeReferenceKind.ErrorString,
				_ => throw new ArgumentException("Invalid input", nameof(input))
			};

			input = input.Substring(2);

			if (kind == CodeReferenceKind.Namespace || kind == CodeReferenceKind.ErrorString)
				return new CodeReference(kind, input, null, null);

			var firstBracket = input.IndexOf('(');

			var lastDot = input.LastIndexOf('.', firstBracket < 0 ? input.Length - 1 : firstBracket - 1);
			if (kind == CodeReferenceKind.Type)
			{
				ns = input.Substring(0, lastDot);
				type = input.Substring(lastDot + 1);
			}
			else
			{
				var last2Dot = input.LastIndexOf('.', lastDot - 1);
				ns = input.Substring(0, last2Dot);
				type = input.Substring(last2Dot + 1, lastDot - last2Dot - 1);
				memberName = input.Substring(lastDot + 1);
			}

			return new CodeReference(kind, ns, type, memberName);
		}

		public override string ToString()
		{
			return Kind switch
			{
				CodeReferenceKind.Namespace => $"{KindChar}:{Namespace}",
				CodeReferenceKind.ErrorString => $"{KindChar}:{Namespace}",
				CodeReferenceKind.Type => $"T:{Namespace}.{TypeName}",
				_ => $"{KindChar}:{Namespace}.{TypeName}.{MemberName}"
			};
		}
	}
}