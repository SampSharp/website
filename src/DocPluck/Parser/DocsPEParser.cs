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
using System.Linq;
using System.Reflection.Metadata;
using DocPluck.Reflection;

namespace DocPluck.Parser
{
	internal class DocsPEParser
	{
		private readonly DocsInfoProvider _infoProvider;
		private readonly MetadataReader _reader;
		private readonly SignatureTypeProvider _signatureTypeProvider;
		private bool _didParse;

		private DocsModuleInfo _module;

		public DocsPEParser(MetadataReader reader)
		{
			_reader = reader ?? throw new ArgumentNullException(nameof(reader));
			_infoProvider = new DocsInfoProvider(reader);
			_signatureTypeProvider = new SignatureTypeProvider(_infoProvider);
		}

		private static bool IsGeneratedTypeName(string name)
		{
			return name.StartsWith("<") ||
			       name.StartsWith("__StaticArrayInitTypeSize=");
		}

		private bool GetConstant(ConstantHandle handle, out object value)
		{
			if (handle.IsNil)
			{
				value = null;
				return false;
			}

			var cc = _reader.GetConstant(handle);
			var typeCode = cc.TypeCode;
			var defValue = typeCode == ConstantTypeCode.NullReference ? null : _reader.GetBlobBytes(cc.Value);

			if (defValue == null)
			{
				value = null;
				return false;
			}

			value = typeCode switch
			{
				ConstantTypeCode.Int32 => BitConverter.ToInt32(defValue),
				ConstantTypeCode.Boolean => BitConverter.ToBoolean(defValue),
				ConstantTypeCode.Char => BitConverter.ToChar(defValue),
				ConstantTypeCode.SByte => (sbyte) defValue[0],
				ConstantTypeCode.Byte => defValue[0],
				ConstantTypeCode.Int16 => BitConverter.ToInt16(defValue),
				ConstantTypeCode.UInt16 => BitConverter.ToUInt16(defValue),
				ConstantTypeCode.UInt32 => BitConverter.ToUInt32(defValue),
				ConstantTypeCode.Int64 => BitConverter.ToInt64(defValue),
				ConstantTypeCode.UInt64 => BitConverter.ToUInt64(defValue),
				ConstantTypeCode.Single => BitConverter.ToSingle(defValue),
				ConstantTypeCode.Double => BitConverter.ToDouble(defValue),
				ConstantTypeCode.String => BitConverter.ToString(defValue),
				_ => (object) null
			};
			return true;
		}

		private DocsPropertyInfo ParseProperty(DocsTypeInfo type, PropertyDefinitionHandle handle)
		{
			var def = _reader.GetPropertyDefinition(handle);

			var sig = def.DecodeSignature(_signatureTypeProvider, type);

			return new DocsPropertyInfo
			{
				Name = _reader.GetString(def.Name),
				DeclaringType = type,
				CustomAttributes = ParseCustomAttributes(def.GetCustomAttributes()),
				PropertyType = sig.ReturnType,
				IndexParameters = sig.ParameterTypes == null || sig.ParameterTypes.Length == 0
					? null
					: sig.ParameterTypes.Select(p => new DocsParameterInfo
					{
						ParameterType = p
					}).ToArray()
			};
		}

		private DocsEventInfo ParseEvent(DocsTypeInfo type, EventDefinitionHandle handle)
		{
			var def = _reader.GetEventDefinition(handle);
			return new DocsEventInfo
			{
				Name = _reader.GetString(def.Name),
				DeclaringType = type,
				EventHandlerType = _infoProvider.ResolveType(def.Type),
				CustomAttributes = ParseCustomAttributes(def.GetCustomAttributes())
			};
		}

		private DocsMethodInfo ParseMethod(DocsTypeInfo type, MethodDefinitionHandle handle)
		{
			var def = _reader.GetMethodDefinition(handle);
			var methodName = _reader.GetString(def.Name);

			if (methodName.StartsWith("<")) // LINQ method
				return null;

			if (methodName == ".cctor") // static initializer
				return null;

			// Remove generic type tick/number
			var tickIndex = methodName.IndexOf('`');
			if (tickIndex >= 0)
				methodName = methodName.Substring(0, tickIndex);

			var paramCollection = def.GetParameters();

			var result = new DocsMethodInfo
			{
				Name = methodName,
				DeclaringType = type,
				Attributes = def.Attributes,
				CustomAttributes = ParseCustomAttributes(def.GetCustomAttributes()),
				GenericArguments = ParseGenericArguments(def.GetGenericParameters(), true)
			};

			var signature = def.DecodeSignature(_signatureTypeProvider, result);

			result.Parameters = new DocsParameterInfo[Math.Min(paramCollection.Count, signature.ParameterTypes.Length)];
			result.ReturnType = signature.ReturnType;

			var index = 0;
			foreach (var paramHandle in paramCollection)
			{
				if (index >= signature.ParameterTypes.Length)
					continue; // This is funky, something to do with overloads with/without generics or something

				var parameter = _reader.GetParameter(paramHandle);

				var pInfo = result.Parameters[index] = new DocsParameterInfo
				{
					Name = _reader.GetString(parameter.Name),
					Attributes = parameter.Attributes,
					ParameterType = signature.ParameterTypes[index],
					CustomAttributes = ParseCustomAttributes(parameter.GetCustomAttributes())
				};

				if (GetConstant(parameter.GetDefaultValue(), out var defValue))
				{
					pInfo.HasDefaultValue = true;
					pInfo.DefaultValue = defValue;
				}

				index++;
			}

			return result;
		}

		private DocsTypeInfo[] ParseGenericArguments(GenericParameterHandleCollection genericParameters, bool isMethod)
		{
			return genericParameters.Count == 0
				? null
				: genericParameters
					.Select(_reader.GetGenericParameter)
					.Select(p =>
					{
						return new DocsTypeInfo
						{
							Name = _reader.GetString(p.Name),
							IsGenericTypeParameter = !isMethod,
							IsGenericMethodParameter = isMethod,
							GenericParameterAttributes = p.Attributes,
							GenericParameterConstraints = p.GetConstraints()
								.Select(cHandle => _reader.GetGenericParameterConstraint(cHandle))
								.Select(c => c.Type)
								.Select(_infoProvider.ResolveType)
								.ToArray()
						};
					}).ToArray();
		}

		private DocsFieldInfo ParseField(DocsTypeInfo type, FieldDefinitionHandle handle)
		{
			var def = _reader.GetFieldDefinition(handle);
			var fieldName = _reader.GetString(def.Name);

			if (fieldName.EndsWith("__BackingField"))
				return null;

			var sign = def.DecodeSignature(_signatureTypeProvider, type);

			var fld = new DocsFieldInfo
			{
				Name = fieldName,
				Attributes = def.Attributes,
				FieldType = sign,
				CustomAttributes = ParseCustomAttributes(def.GetCustomAttributes()),
				DeclaringType = type
			};

			if (type.IsEnum && GetConstant(def.GetDefaultValue(), out var defValue))
				fld.EnumValue = defValue;

			return fld;
		}

		private void ParseTypeMembers(DocsTypeInfo type, TypeDefinition def)
		{
			// Parse implemented interfaces
			type.InterfaceImplementations.AddRange(def.GetInterfaceImplementations()
				.Select(n => _reader.GetInterfaceImplementation(n))
				.Select(n => n.Interface)
				.Select(_infoProvider.ResolveType));

			// Parse properties
			type.Properties.AddRange(def.GetProperties()
				.Select(n => ParseProperty(type, n)));

			// Parse nested types
			type.NestedTypes.AddRange(def.GetNestedTypes()
				.Where(n => !IsGeneratedTypeName(_reader.GetString(_reader.GetTypeDefinition(n).Name)))
				.Select(n => (EntityHandle) n)
				.Select(_infoProvider.ResolveType));

			// Parse events
			type.Events.AddRange(def.GetEvents()
				.Select(n => ParseEvent(type, n)));

			// Parse methods
			foreach (var method in def.GetMethods()
				.Select(n => ParseMethod(type, n))
				.Where(method => method != null))
				if (method.Name == ".ctor")
					type.Constructors.Add(new DocsConstructorInfo
					{
						Name = method.Name,
						DeclaringType = type,
						Attributes = method.Attributes,
						Parameters = method.Parameters,
						CustomAttributes = method.CustomAttributes
					});
				else
					type.Methods.Add(method);

			// Parse fields
			foreach (var field in def.GetFields()
				.Select(n => ParseField(type, n))
				.Where(field => field != null))
				if (field.Name == "value__" && type.IsEnum)
					type.UnderlyingEnumType = field.FieldType;
				else
					type.Fields.Add(field);
		}

		private DocsCustomAttributeData ParseCustomAttribute(CustomAttributeHandle handle)
		{
			var customAttribute = _reader.GetCustomAttribute(handle);

			DocsTypeInfo attributeType = null;

			switch (customAttribute.Constructor.Kind)
			{
				case HandleKind.MemberReference:
				{
					var memberReference = _reader.GetMemberReference((MemberReferenceHandle) customAttribute.Constructor);
					attributeType = _infoProvider.ResolveType(memberReference.Parent);
					break;
				}
				case HandleKind.MethodDefinition:
				{
					var def = _reader.GetMethodDefinition((MethodDefinitionHandle) customAttribute.Constructor);
					attributeType = _infoProvider.ResolveType(def.GetDeclaringType());
					break;
				}
			}

			var value = customAttribute.DecodeValue(_signatureTypeProvider);

			return new DocsCustomAttributeData(attributeType,
				value.FixedArguments.Select(a => new DocsCustomAttributeArgument(a.Type, a.Value)).ToArray(),
				value.NamedArguments.Select(a => new DocsCustomAttributeNamedArgument(a.Type, a.Value, a.Name)).ToArray());
		}

		private DocsCustomAttributeData[] ParseCustomAttributes(CustomAttributeHandleCollection customAttributes)
		{
			try
			{
				return customAttributes.Select(ParseCustomAttribute).ToArray();
			}
			catch (BadImageFormatException)
			{
				// It is thrown here, but I don't know why :(
				// https://github.com/dotnet/runtime/blob/0ac5dc9f7a8524dcec83de8f02a738d51b26f5f3/src/libraries/System.Reflection.Metadata/src/System/Reflection/Metadata/Ecma335/CustomAttributeDecoder.cs#L101
				return new DocsCustomAttributeData[0];
			}
		}

		private void ParseType(TypeDefinitionHandle handle)
		{
			var def = _reader.GetTypeDefinition(handle);
			var name = _reader.GetString(def.Name);
			var ns = _reader.GetString(def.Namespace);

			if (IsGeneratedTypeName(name))
				return;

			// Remove generic type tick/number
			var tickIndex = name.IndexOf('`');
			if (tickIndex >= 0)
				name = name.Substring(0, tickIndex);

			// Parse type
			var info = _infoProvider.ResolveType(handle);

			info.Name = name;
			info.Namespace = ns;
			info.Attributes = def.Attributes;
			info.DeclaringType = _infoProvider.ResolveType(def.GetDeclaringType());
			info.GenericArguments = ParseGenericArguments(def.GetGenericParameters(), false);
			info.IsGenericTypeDefinition = info.GenericArguments != null;
			info.BaseType = _infoProvider.ResolveType(def.BaseType);

			info.CustomAttributes = ParseCustomAttributes(def.GetCustomAttributes());
			info.Assembly = ParseAssembly();
			info.Module = ParseModule();

			ParseTypeMembers(info, def);

			ParseNamespace(def.NamespaceDefinition);
		}

		private void ParseNamespace(NamespaceDefinitionHandle handle)
		{
			var ns = _infoProvider.ResolveNamespace(handle);

			if (ns == null || ns.Types != null)
				return;

			var def = _reader.GetNamespaceDefinition(handle);

			ns.Parent = _infoProvider.ResolveNamespace(def.Parent);
			ns.Types = def.TypeDefinitions.Select(t => _infoProvider.ResolveType(t)).ToArray();
			ns.ExportedTypes = def.ExportedTypes.Select(t => _infoProvider.ResolveType(t)).ToArray();
			ns.Namespaces = def.NamespaceDefinitions.Select(_infoProvider.ResolveNamespace).ToArray();
			ns.Name = _reader.GetString(def.Name);
		}

		private DocsAssemblyInfo ParseAssembly()
		{
			return ParseModule().Assembly;
		}

		private DocsModuleInfo ParseModule()
		{
			if (_module != null)
				return _module;

			var mod = _reader.GetModuleDefinition();
			var asm = _reader.GetAssemblyDefinition();


			//asm.Name
			_module = new DocsModuleInfo
			{
				Name = _reader.GetString(mod.Name),
				CustomAttributes = ParseCustomAttributes(mod.GetCustomAttributes())
			};

			_module.Assembly = new DocsAssemblyInfo
			{
				CustomAttributes = ParseCustomAttributes(asm.GetCustomAttributes()),
				Name = asm.GetAssemblyName(),
				Module = _module
			};

			return _module;
		}

		private void RemoveSpecialMethod(DocsTypeInfo type, string methodName, Action<DocsMethodInfo> handler)
		{
			var method = type.Methods.FirstOrDefault(m => m.Name == methodName);

			if (method == null || !method.IsSpecialName) return;
			handler(method);
			type.Methods.Remove(method);
		}

		private bool ParametersEquals(DocsParameterInfo[] lhs, DocsParameterInfo[] rhs)
		{
			return lhs.Zip(rhs, (a, b) => a.ParameterType.Equals(b.ParameterType)).All(n => n);
		}

		private void RemovePropertyMethods(DocsTypeInfo type)
		{
			foreach (var property in type.Properties)
			{
				if (property.IndexParameters != null)
				{
					property.GetMethod = type.Methods.FirstOrDefault(m => m.Name == "get_Item" && ParametersEquals(m.Parameters, property.IndexParameters));
					property.SetMethod = type.Methods.FirstOrDefault(m => m.Name == "set_Item" && ParametersEquals(m.Parameters, property.IndexParameters));
					
					if (property.GetMethod != null)
						type.Methods.Remove(property.GetMethod);
					if (property.SetMethod != null)
						type.Methods.Remove(property.SetMethod);

					if (property.GetMethod != null)
						property.IndexParameters = property.GetMethod.Parameters;

					if (property.GetMethod == null && property.SetMethod != null)
					{
						property.IndexParameters = property
							.SetMethod
							.Parameters
							.Take(property.IndexParameters.Length)
							.ToArray();
					}
				}
				else
				{
					RemoveSpecialMethod(type, "get_" + property.Name, m => property.GetMethod = m);
					RemoveSpecialMethod(type, "set_" + property.Name, m => property.SetMethod = m);
				}

			}
		}

		private void RemoveEventMethods(DocsTypeInfo type)
		{
			foreach (var @event in type.Events)
			{
				RemoveSpecialMethod(type, $"add_{@event.Name}", m => @event.AddMethod = m);
				RemoveSpecialMethod(type, $"remove_{@event.Name}", m => @event.RemoveMethod = m);
			}
		}

		public ParsedAssembly Parse()
		{
			if (_didParse)
				throw new InvalidOperationException("The data has already been parsed.");

			foreach (var type in _reader.TypeDefinitions) ParseType(type);

			// Move property getter and setter methods out of the methods list into the property
			foreach (var t in _infoProvider.Types)
			{
				RemovePropertyMethods(t);
				RemoveEventMethods(t);
			}

			_didParse = true;
			return new ParsedAssembly(_module.Assembly, _module, _infoProvider.Namespaces.ToArray(), _infoProvider.Types.ToArray());
		}
	}
}