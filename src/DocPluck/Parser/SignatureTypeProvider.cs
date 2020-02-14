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
using System.Collections.Immutable;
using System.Linq;
using System.Reflection.Metadata;
using DocPluck.Reflection;

namespace DocPluck.Parser
{
	internal sealed class SignatureTypeProvider : ISignatureTypeProvider<DocsTypeInfo, object>, ICustomAttributeTypeProvider<DocsTypeInfo>
	{
		private static readonly string[] SystemTypes =
		{
			"Boolean",
			"Byte",
			"Char",
			"Double",
			"Int8",
			"UInt8",
			"Int16",
			"UInt16",
			"Int32",
			"UInt32",
			"Int64",
			"UInt64",
			"IntPtr",
			"Object",
			"Single",
			"String",
			"TypedReference",
			"UIntPtr",
			"Void"
		};

		private readonly DocsInfoProvider _infoProvider;

		public SignatureTypeProvider(DocsInfoProvider infoProvider)
		{
			_infoProvider = infoProvider;
		}

		public DocsTypeInfo GetSystemType()
		{
			return FromKnownType(typeof(Type));
		}

		public DocsTypeInfo GetTypeFromSerializedName(string name)
		{
			throw new NotImplementedException("Deserialization from " + name + " not implemented");
		}

		public PrimitiveTypeCode GetUnderlyingEnumType(DocsTypeInfo type)
		{
			if (type.UnderlyingEnumType == null)
				// Some external type we don't know, assume int
				// TODO: Might break deserialization of external enums :(
				return PrimitiveTypeCode.Int32;
			return type.UnderlyingEnumType.Name switch
			{
				"Int8" => PrimitiveTypeCode.SByte,
				"UInt8" => PrimitiveTypeCode.Byte,
				"Int16" => PrimitiveTypeCode.Int16,
				"UInt16" => PrimitiveTypeCode.UInt16,
				"Int32" => PrimitiveTypeCode.Int32,
				"UInt32" => PrimitiveTypeCode.UInt32,
				"Int64" => PrimitiveTypeCode.Int64,
				"UInt64" => PrimitiveTypeCode.UInt64,
				_ => PrimitiveTypeCode.Int32
			};
		}

		public bool IsSystemType(DocsTypeInfo type)
		{
			return type.Namespace == "System" && SystemTypes.Contains(type.Name);
		}

		public DocsTypeInfo GetPrimitiveType(PrimitiveTypeCode typeCode)
		{
			switch (typeCode)
			{
				case PrimitiveTypeCode.Boolean: return FromKnownType(typeof(bool));
				case PrimitiveTypeCode.Byte: return FromKnownType(typeof(byte));
				case PrimitiveTypeCode.Char: return FromKnownType(typeof(char));
				case PrimitiveTypeCode.Double: return FromKnownType(typeof(double));
				case PrimitiveTypeCode.Int16: return FromKnownType(typeof(short));
				case PrimitiveTypeCode.Int32: return FromKnownType(typeof(int));
				case PrimitiveTypeCode.Int64: return FromKnownType(typeof(long));
				case PrimitiveTypeCode.IntPtr: return FromKnownType(typeof(IntPtr));
				case PrimitiveTypeCode.Object: return FromKnownType(typeof(object));
				case PrimitiveTypeCode.SByte: return FromKnownType(typeof(sbyte));
				case PrimitiveTypeCode.Single: return FromKnownType(typeof(float));
				case PrimitiveTypeCode.String: return FromKnownType(typeof(string));
				case PrimitiveTypeCode.TypedReference: return FromKnownType(typeof(TypedReference));
				case PrimitiveTypeCode.UInt16: return FromKnownType(typeof(ushort));
				case PrimitiveTypeCode.UInt32: return FromKnownType(typeof(uint));
				case PrimitiveTypeCode.UInt64: return FromKnownType(typeof(ulong));
				case PrimitiveTypeCode.UIntPtr: return FromKnownType(typeof(UIntPtr));
				case PrimitiveTypeCode.Void: return FromKnownType(typeof(void));
				default: return null;
			}
		}

		public DocsTypeInfo GetTypeFromDefinition(MetadataReader reader, TypeDefinitionHandle handle, byte rawTypeKind = 0)
		{
			return _infoProvider.ResolveType(handle);
		}

		public DocsTypeInfo GetTypeFromReference(MetadataReader reader, TypeReferenceHandle handle, byte rawTypeKind = 0)
		{
			return _infoProvider.ResolveType(handle);
		}

		public DocsTypeInfo GetTypeFromSpecification(MetadataReader reader, object genericContext, TypeSpecificationHandle handle, byte rawTypeKind = 0)
		{
			return _infoProvider.ResolveType(handle);
		}

		public DocsTypeInfo GetSZArrayType(DocsTypeInfo elementType)
		{
			return new DocsTypeInfo
			{
				ElementType = elementType,
				IsArray = true,
				IsExternalReference = true
			};
		}

		public DocsTypeInfo GetPointerType(DocsTypeInfo elementType)
		{
			return new DocsTypeInfo
			{
				ElementType = elementType,
				IsPointer = true,
				IsExternalReference = true
			};
		}

		public DocsTypeInfo GetByReferenceType(DocsTypeInfo elementType)
		{
			return new DocsTypeInfo
			{
				ElementType = elementType,
				IsByRef = true,
				IsExternalReference = true
			};
		}

		public DocsTypeInfo GetGenericMethodParameter(object genericContext, int index)
		{
			var method = genericContext as DocsMethodInfo;

			return new DocsTypeInfo
			{
				IsGenericMethodParameter = true,
				IsExternalReference = true,
				GenericParameterIndex = index,
				Name = method?.GenericArguments != null && method.GenericArguments.Length > index
					? method.GenericArguments[index].Name
					: "T" + index // fallback
			};
		}

		public DocsTypeInfo GetGenericTypeParameter(object genericContext, int index)
		{
			var type = genericContext as DocsTypeInfo;

			if (genericContext is DocsMethodInfo method)
				type = method.DeclaringType;

			return new DocsTypeInfo
			{
				IsGenericTypeParameter = true,
				IsExternalReference = true,
				GenericParameterIndex = index,
				Name = type?.GenericArguments != null && type.GenericArguments.Length > index
					? type.GenericArguments[index].Name
					: "T" + index // fallback
			};
		}

		public DocsTypeInfo GetPinnedType(DocsTypeInfo elementType)
		{
			return elementType; // Not implemented...
		}

		public DocsTypeInfo GetGenericInstantiation(DocsTypeInfo genericType, ImmutableArray<DocsTypeInfo> typeArguments)
		{
			var result = new DocsTypeInfo
			{
				Name = genericType.Name,
				Namespace = genericType.Namespace,
				Attributes = genericType.Attributes,
				BaseType = genericType.BaseType,
				DeclaringType = genericType.DeclaringType,
				ElementType = genericType.ElementType,
				GenericArguments = typeArguments.ToArray(),
				IsExternalReference = genericType.IsExternalReference,
				IsByRef = genericType.IsByRef,
				IsArray = genericType.IsArray,
				IsGenericType = true
			};

			result.InterfaceImplementations.AddRange(genericType.InterfaceImplementations);
			result.Constructors.AddRange(genericType.Constructors);
			result.Events.AddRange(genericType.Events);
			result.Fields.AddRange(genericType.Fields);
			result.Properties.AddRange(genericType.Properties);
			result.NestedTypes.AddRange(genericType.NestedTypes); // maybe not this one?
			result.Methods.AddRange(genericType.Methods);

			return result;
		}

		public DocsTypeInfo GetModifiedType(DocsTypeInfo modifierType, DocsTypeInfo unmodifiedType, bool isRequired)
		{
			// throw new NotImplementedException();
			return modifierType;
		}

		public DocsTypeInfo GetArrayType(DocsTypeInfo elementType, ArrayShape shape)
		{
			// throw new NotImplementedException();
			return elementType;
		}

		public DocsTypeInfo GetFunctionPointerType(MethodSignature<DocsTypeInfo> signature)
		{
			// throw new NotImplementedException();
			return new DocsTypeInfo();
		}

		private DocsTypeInfo FromKnownType(Type type)
		{
			return type == null
				? null
				: new DocsTypeInfo
				{
					Attributes = type.Attributes,
					IsExternalReference = true,
					Name = type.Name,
					Namespace = type.Namespace
				};
		}
	}
}