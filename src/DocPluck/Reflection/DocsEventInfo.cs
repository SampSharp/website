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
	/// Discovers the attributes of an event and provides access to event metadata.
	/// </summary>
	public class DocsEventInfo : DocsMemberInfo
	{
		/// <summary>
		/// Gets or sets the type of the event handler.
		/// </summary>
		[DataMember]
		public DocsTypeInfo EventHandlerType { get; set; }

		/// <summary>
		/// Gets or sets the add method.
		/// </summary>
		[DataMember]
		public DocsMethodInfo AddMethod { get; set; }

		/// <summary>
		/// Gets or sets the remove method.
		/// </summary>
		[DataMember]
		public DocsMethodInfo RemoveMethod { get; set; }

		public override DocsAccessibilityLevel AccessibilityLevel => AddMethod?.AccessibilityLevel ?? RemoveMethod?.AccessibilityLevel ?? DocsAccessibilityLevel.Unknown;
		
		/// <summary>
		/// Gets the documentation of this event.
		/// </summary>
		[DataMember]
		public XmlDocEventInfo Documentation { get; set; }
	}
}