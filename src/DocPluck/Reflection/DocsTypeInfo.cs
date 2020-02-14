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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using DocPluck.XmlInfo;

namespace DocPluck.Reflection
{
	/// <summary>
	/// Represents type declarations: class types, interface types, array types, value types, enumeration types, type
	/// parameters, generic type definitions, and open or closed constructed generic types.
	/// </summary>
	public class DocsTypeInfo : DocsMemberInfo
	{
		[DataMember]
		private string _namespace;

		/// <summary>
		/// Gets the kind of this type.
		/// </summary>
		public DocsTypeInfoKind Kind
		{
			get
			{
				if (Attributes.HasFlag(TypeAttributes.Interface))
					return DocsTypeInfoKind.Interface;
				if (Attributes.HasFlag(TypeAttributes.Class))
					return DocsTypeInfoKind.Class;
				if (IsDelegate)
					return DocsTypeInfoKind.Delegate;
				if (IsEnum)
					return DocsTypeInfoKind.Enum;

				return DocsTypeInfoKind.Struct;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this type is an interface.
		/// </summary>
		public bool IsInterface => Kind == DocsTypeInfoKind.Interface;

		/// <summary>
		/// Gets or sets a value indicating whether this type is an array.
		/// </summary>
		[DataMember]
		public bool IsArray { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this type is by reference.
		/// </summary>
		[DataMember]
		public bool IsByRef { get; set; }

		/// <summary>
		/// Gets a value indicating whether this type is an enum.
		/// </summary>
		public bool IsEnum => BaseType != null && BaseType.IsExternalReference && BaseType.Name == "Enum" && BaseType.Namespace == "System";

		/// <summary>
		/// Gets a value indicating whether this type is a delegate.
		/// </summary>
		public bool IsDelegate => BaseType != null && BaseType.IsExternalReference && BaseType.Name == "MulticastDelegate" && BaseType.Namespace == "System";

		//public bool IsOut { get;  set; } // is this possible?		

		/// <summary>
		/// Gets a value that indicates whether the current type represents a type parameter in the definition of a generic type or
		/// method.
		/// </summary>
		public bool IsGenericParameter => IsGenericTypeParameter || IsGenericMethodParameter;

		/// <summary>
		/// Gets a value that indicates whether the current type represents a type parameter in the definition of a generic type.
		/// </summary>
		[DataMember]
		public bool IsGenericTypeParameter { get; set; }

		/// <summary>
		/// Gets a value that indicates whether the current Type represents a type parameter in the definition of a generic method.
		/// </summary>
		[DataMember]
		public bool IsGenericMethodParameter { get; set; }

		/// <summary>
		/// Gets a value indicating whether the current type is a generic type.
		/// </summary>
		[DataMember]
		public bool IsGenericType { get; set; }

		/// <summary>
		/// Gets a value indicating whether the current type represents a generic type definition, from which other generic types
		/// can be constructed.
		/// </summary>
		[DataMember]
		public bool IsGenericTypeDefinition { get; set; }

		/// <summary>
		/// Get the index of this generic parameter.
		/// </summary>
		[DataMember]
		public int GenericParameterIndex { get; set; }

		/// <summary>
		/// Gets the type of the object encompassed or referred to by the current array, pointer or reference type.
		/// </summary>
		[DataMember]
		public DocsTypeInfo ElementType { get; set; }

		/// <summary>
		/// Gets the underlying type of this enumeration type.
		/// </summary>
		[DataMember]
		public DocsTypeInfo UnderlyingEnumType { get; set; }

		/// <summary>
		/// Gets a value indicating whether the current type encompasses or refers to another type; that is, whether the current
		/// type is an array, a pointer, or is passed by reference.
		/// </summary>
		public bool HasElementType => ElementType != null;

		/// <summary>
		/// Gets an array of type objects that represent the type arguments of a closed generic type or the type parameters of a
		/// generic type definition.
		/// </summary>
		[DataMember]
		public DocsTypeInfo[] GenericArguments { get; set; }

		/// <summary>
		/// Gets an array of type objects that represent the constraints on the current generic type parameter.
		/// </summary>
		[DataMember]
		public DocsTypeInfo[] GenericParameterConstraints { get; set; } // only if IsGenericParameter

		/// <summary>
		/// Gets the type from which the current type directly inherits.
		/// </summary>
		[DataMember]
		public DocsTypeInfo BaseType { get; set; }

		/// <summary>
		/// Gets a value indicating whether this type is value type.
		/// </summary>
		[DataMember]
		public bool IsValueType { get; set; }

		/// <summary>
		/// Gets the attributes of this type.
		/// </summary>
		[DataMember]
		public TypeAttributes Attributes { get; set; }

		/// <summary>
		/// Gets the generic parameter attributes of this type.
		/// </summary>
		[DataMember]
		public GenericParameterAttributes GenericParameterAttributes { get; set; }

		/// <summary>
		/// Gets a value indicating whether this type is a nested type.
		/// </summary>
		public bool IsNested => DeclaringType != null;

		/// <summary>
		/// Gets a value indicating whether this type is a pointer type.
		/// </summary>
		[DataMember]
		public bool IsPointer { get; set; }

		/// <summary>
		/// Gets the assembly in which the type is declared. For generic types, gets the assembly in which the generic type is
		/// defined.
		/// </summary>
		[DataMember]
		public DocsAssemblyInfo Assembly { get; set; }
		
		/// <summary>
		/// Gets the module of this type.
		/// </summary>
		[DataMember]
		public DocsModuleInfo Module { get; set; }

		/// <summary>
		/// Gets a value indicating whether this type does not belong to the parsed assembly. External reference types are
		/// incomplete and provide no data about their members or attributes and provide incomplete metadata.
		/// </summary>
		[DataMember]
		public bool IsExternalReference { get; set; } // not fully decorated, type is reference of type outside of scanned module

		/// <summary>
		/// Gets the nested types of this type.
		/// </summary>
		[DataMember]
		public List<DocsTypeInfo> NestedTypes { get; } = new List<DocsTypeInfo>();
		/// <summary>
		/// Gets the interface implementations of this type.
		/// </summary>
		[DataMember]
		public List<DocsTypeInfo> InterfaceImplementations { get; } = new List<DocsTypeInfo>();
		/// <summary>
		/// Gets the properties of this type.
		/// </summary>
		[DataMember]
		public List<DocsPropertyInfo> Properties { get; } = new List<DocsPropertyInfo>();
		/// <summary>
		/// Gets the methods of this type.
		/// </summary>
		[DataMember]
		public List<DocsMethodInfo> Methods { get; } = new List<DocsMethodInfo>();
		/// <summary>
		/// Gets the constructors of this type.
		/// </summary>
		[DataMember]
		public List<DocsConstructorInfo> Constructors { get; } = new List<DocsConstructorInfo>();
		/// <summary>
		/// Gets the events of this type.
		/// </summary>
		[DataMember]
		public List<DocsEventInfo> Events { get; } = new List<DocsEventInfo>();
		/// <summary>
		/// Gets the fields of this type.
		/// </summary>
		[DataMember]
		public List<DocsFieldInfo> Fields { get; } = new List<DocsFieldInfo>();

		/// <summary>
		/// Gets the full name of this type.
		/// </summary>
		public string FullName => $"{(DeclaringType == null ? Namespace : DeclaringType.FullName)}.{Name}";

		public override DocsAccessibilityLevel AccessibilityLevel =>
			IsPublic || IsNestedPublic ? DocsAccessibilityLevel.Public :
			IsNotPublic || IsNestedAssembly ? DocsAccessibilityLevel.Internal :
			IsNestedPrivate ? DocsAccessibilityLevel.Private :
			IsNestedFamily ? DocsAccessibilityLevel.Protected :
			IsNestedFamANDAssem ? DocsAccessibilityLevel.PrivateProtected :
			IsNestedFamORAssem ? DocsAccessibilityLevel.ProtectedInternal : DocsAccessibilityLevel.Unknown;
		
		public override string Name
		{
			get => (HasElementType ? ElementType.Name : base.Name) + (IsPointer ? "*" : string.Empty) + (IsByRef ? "&" : string.Empty) + (IsArray ? "[]" : string.Empty);
			set => base.Name = value;
		}

		/// <summary>
		/// Gets or sets the namespace of this type.
		/// </summary>
		public string Namespace
		{
			get => HasElementType ? ElementType.Namespace : _namespace;
			set => _namespace = value;
		}

		#region Attribute based flags

		/// <inheritdoc cref="Type.IsAbstract"/>
		public bool IsAbstract => (uint) (Attributes & TypeAttributes.Abstract) > 0U;
		
		/// <inheritdoc cref="Type.IsImport"/>
		public bool IsImport => (uint) (Attributes & TypeAttributes.Import) > 0U;
		
		/// <inheritdoc cref="Type.IsSealed"/>
		public bool IsSealed => (uint) (Attributes & TypeAttributes.Sealed) > 0U;
		
		/// <inheritdoc cref="Type.IsSpecialName"/>
		public bool IsSpecialName => (uint) (Attributes & TypeAttributes.SpecialName) > 0U;
		
		/// <inheritdoc cref="Type.IsClass"/>
		public bool IsClass => (Attributes & TypeAttributes.ClassSemanticsMask) == TypeAttributes.NotPublic && !IsValueType;
		
		/// <inheritdoc cref="Type.IsNestedAssembly"/>
		public bool IsNestedAssembly => (Attributes & TypeAttributes.VisibilityMask) == TypeAttributes.NestedAssembly;
		
		/// <inheritdoc cref="Type.IsNestedFamANDAssem"/>
		public bool IsNestedFamANDAssem => (Attributes & TypeAttributes.VisibilityMask) == TypeAttributes.NestedFamANDAssem;
		
		/// <inheritdoc cref="Type.IsNestedFamily"/>
		public bool IsNestedFamily => (Attributes & TypeAttributes.VisibilityMask) == TypeAttributes.NestedFamily;
		
		/// <inheritdoc cref="Type.IsNestedFamORAssem"/>
		public bool IsNestedFamORAssem => (Attributes & TypeAttributes.VisibilityMask) == TypeAttributes.VisibilityMask;
		
		/// <inheritdoc cref="Type.IsNestedPrivate"/>
		public bool IsNestedPrivate => (Attributes & TypeAttributes.VisibilityMask) == TypeAttributes.NestedPrivate;
		
		/// <inheritdoc cref="Type.IsNestedPublic"/>
		public bool IsNestedPublic => (Attributes & TypeAttributes.VisibilityMask) == TypeAttributes.NestedPublic;
		
		/// <inheritdoc cref="Type.IsNotPublic"/>
		public bool IsNotPublic => (Attributes & TypeAttributes.VisibilityMask) == TypeAttributes.NotPublic;
		
		/// <inheritdoc cref="Type.IsPublic"/>
		public bool IsPublic => (Attributes & TypeAttributes.VisibilityMask) == TypeAttributes.Public;
		
		/// <inheritdoc cref="Type.IsAutoLayout"/>
		public bool IsAutoLayout => (Attributes & TypeAttributes.LayoutMask) == TypeAttributes.NotPublic;
		
		/// <inheritdoc cref="Type.IsExplicitLayout"/>
		public bool IsExplicitLayout => (Attributes & TypeAttributes.LayoutMask) == TypeAttributes.ExplicitLayout;
		
		/// <inheritdoc cref="Type.IsLayoutSequential"/>
		public bool IsLayoutSequential => (Attributes & TypeAttributes.LayoutMask) == TypeAttributes.SequentialLayout;
		
		/// <inheritdoc cref="Type.IsAnsiClass"/>
		public bool IsAnsiClass => (Attributes & TypeAttributes.StringFormatMask) == TypeAttributes.NotPublic;
		
		/// <inheritdoc cref="Type.IsAutoClass"/>
		public bool IsAutoClass => (Attributes & TypeAttributes.StringFormatMask) == TypeAttributes.AutoClass;
		
		/// <inheritdoc cref="Type.IsUnicodeClass"/>
		public bool IsUnicodeClass => (Attributes & TypeAttributes.StringFormatMask) == TypeAttributes.UnicodeClass;
		
		#endregion

		/// <summary>
		/// Gets the documentation of this type.
		/// </summary>
		[DataMember]
		public XmlDocTypeInfo Documentation { get; set; }

		/// <summary>
		/// Gets a value indicating whether this type represents the same type as the specified <paramref name="other"/> type.
		/// </summary>
		/// <param name="other">The other type to compare this type to.</param>
		/// <returns><c>true</c> if this type equals the specified other type; otherwise <c>false</c>.</returns>
		public bool Equals(DocsTypeInfo other)
		{
			return other != null &&
			       Namespace == other.Namespace &&
			       Name == other.Name &&
			       Attributes == other.Attributes &&
			       IsGenericType == other.IsGenericType &&
			       IsGenericTypeParameter == other.IsGenericTypeParameter &&
			       IsGenericMethodParameter == other.IsGenericMethodParameter &&
			       IsGenericTypeDefinition == other.IsGenericTypeDefinition &&
			       IsGenericParameter == other.IsGenericParameter &&
			       GenericParameterAttributes == other.GenericParameterAttributes &&
			       GenericParameterIndex == other.GenericParameterIndex &&
			       CompareTypes(GenericArguments, other.GenericArguments) &&
			       CompareTypes(GenericParameterConstraints, other.GenericParameterConstraints) &&
			       (DeclaringType == null && other.DeclaringType == null || DeclaringType != null && DeclaringType.Equals(other.DeclaringType)) &&
			       AccessibilityLevel == other.AccessibilityLevel &&
			       IsExternalReference == other.IsExternalReference &&
			       IsValueType == other.IsValueType &&
			       Module == other.Module &&
			       Assembly == other.Assembly &&
			       HasElementType == other.HasElementType &&
			       (!HasElementType || ElementType.Equals(other.ElementType)) &&
			       IsArray == other.IsArray &&
			       IsByRef == other.IsByRef
				;
		}

		private static bool CompareTypes(DocsTypeInfo[] lhs, DocsTypeInfo[] rhs)
		{
			if (lhs == null && rhs == null)
				return true;

			if (lhs == null || rhs == null)
				return false;

			return lhs.Zip(rhs, (a, b) => a.Equals(b)).All(n => n);
		}
	}
}