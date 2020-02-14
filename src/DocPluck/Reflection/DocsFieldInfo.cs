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

using System.Reflection;
using System.Runtime.Serialization;
using DocPluck.XmlInfo;

namespace DocPluck.Reflection
{
	/// <summary>
	/// Discovers the attributes of a field and provides access to field metadata.
	/// </summary>
	public class DocsFieldInfo : DocsMemberInfo
	{
		/// <summary>
		/// Gets or sets the type of this field.
		/// </summary>
		[DataMember]
		public DocsTypeInfo FieldType { get; set; }

		/// <summary>
		/// Gets or sets the attributes of this field.
		/// </summary>
		[DataMember]
		public FieldAttributes Attributes { get; set; }

		/// <summary>
		/// Gets or sets the value this enum value represents.
		/// </summary>
		/// <remarks>This value is <c>null</c> if this field is not an enum value.</remarks>
		[DataMember]
		public object EnumValue { get; set; }

		public override DocsAccessibilityLevel AccessibilityLevel =>
			IsPublic ? DocsAccessibilityLevel.Public :
			IsAssembly ? DocsAccessibilityLevel.Internal :
			IsPrivate ? DocsAccessibilityLevel.Private :
			IsFamily ? DocsAccessibilityLevel.Protected :
			IsFamilyAndAssembly ? DocsAccessibilityLevel.PrivateProtected :
			IsFamilyOrAssembly ? DocsAccessibilityLevel.ProtectedInternal : DocsAccessibilityLevel.Unknown;

		#region Attribute based flags

		/// <inheritdoc cref="FieldInfo.IsInitOnly" />
		public bool IsInitOnly => (uint) (Attributes & FieldAttributes.InitOnly) > 0U;

		/// <inheritdoc cref="FieldInfo.IsLiteral" />
		public bool IsLiteral => (uint) (Attributes & FieldAttributes.Literal) > 0U;

		/// <inheritdoc cref="FieldInfo.IsNotSerialized" />
		public bool IsNotSerialized => (uint) (Attributes & FieldAttributes.NotSerialized) > 0U;

		/// <inheritdoc cref="FieldInfo.IsPinvokeImpl" />
		public bool IsPinvokeImpl => (uint) (Attributes & FieldAttributes.PinvokeImpl) > 0U;

		/// <inheritdoc cref="FieldInfo.IsSpecialName" />
		public bool IsSpecialName => (uint) (Attributes & FieldAttributes.SpecialName) > 0U;

		/// <inheritdoc cref="FieldInfo.IsStatic" />
		public bool IsStatic => (uint) (Attributes & FieldAttributes.Static) > 0U;

		/// <inheritdoc cref="FieldInfo.IsAssembly" />
		public bool IsAssembly => (Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.Assembly;

		/// <inheritdoc cref="FieldInfo.IsFamily" />
		public bool IsFamily => (Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.Family;

		/// <inheritdoc cref="FieldInfo.IsFamilyAndAssembly" />
		public bool IsFamilyAndAssembly => (Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.FamANDAssem;

		/// <inheritdoc cref="FieldInfo.IsFamilyOrAssembly" />
		public bool IsFamilyOrAssembly => (Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.FamORAssem;

		/// <inheritdoc cref="FieldInfo.IsPrivate" />
		public bool IsPrivate => (Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.Private;

		/// <inheritdoc cref="FieldInfo.IsPublic" />
		public bool IsPublic => (Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.Public;
		
		#endregion

		/// <summary>
		/// Gets the documentation of this field.
		/// </summary>
		[DataMember]
		public XmlDocFieldInfo Documentation { get; set; }

	}
}