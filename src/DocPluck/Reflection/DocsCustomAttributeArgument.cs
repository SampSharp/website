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

namespace DocPluck.Reflection
{
	/// <summary>
	/// Represents an argument of a custom attribute in the reflection-only context, or an element of an array argument.
	/// </summary>
	[DataContract]
	public struct DocsCustomAttributeArgument
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DocsCustomAttributeArgument" /> struct.
		/// </summary>
		/// <param name="type">The type of the custom attribute argument.</param>
		/// <param name="value">The value of the custom attribute argument.</param>
		public DocsCustomAttributeArgument(DocsTypeInfo type, object value)
		{
			Type = type;
			Value = value;
		}

		/// <summary>
		/// Gets the type of the custom attribute argument.
		/// </summary>
		[DataMember]
		public DocsTypeInfo Type { get; }

		/// <summary>
		/// Gets the value of the custom attribute argument.
		/// </summary>
		[DataMember]
		public object Value { get; }
	}
}