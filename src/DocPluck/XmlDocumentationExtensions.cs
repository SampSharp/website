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

using System.Linq;
using DocPluck.Reflection;
using DocPluck.XmlInfo;

namespace DocPluck
{
	public static class XmlDocumentationExtensions
	{
		public static XmlDocNamespaceInfo Get(this XmlDocumentation documentation, DocsNamespaceInfo @namespace)
		{
			if (documentation == null || @namespace == null)
				return null;

			return documentation.Members.OfType<XmlDocNamespaceInfo>().FirstOrDefault(n => n.Name == @namespace.Name);
		}
		
		public static XmlDocTypeInfo Get(this XmlDocumentation documentation, DocsTypeInfo type)
		{
			if (documentation == null || type == null)
				return null;

			var xmlName = GetXmlName(type);
			return documentation.Members.OfType<XmlDocTypeInfo>().FirstOrDefault(n => n.Name == xmlName);
		}
		
		public static XmlDocMethodInfo Get(this XmlDocumentation documentation, DocsMethodInfo method)
		{
			if (documentation == null || method == null)
				return null;

			var xmlName = GetXmlName(method);
			return documentation.Members.OfType<XmlDocMethodInfo>().FirstOrDefault(n => n.Name == xmlName);
		}

		public static XmlDocMethodInfo Get(this XmlDocumentation documentation, DocsConstructorInfo method)
		{
			if (documentation == null || method == null)
				return null;

			var xmlName = GetXmlName(method);
			return documentation.Members.OfType<XmlDocMethodInfo>().FirstOrDefault(n => n.Name == xmlName);
		}

		public static XmlDocEventInfo Get(this XmlDocumentation documentation, DocsEventInfo @event)
		{
			if (documentation == null || @event == null)
				return null;

			var xmlName = GetXmlName(@event);
			return documentation.Members.OfType<XmlDocEventInfo>().FirstOrDefault(n => n.Name == xmlName);
		}
		
		public static XmlDocFieldInfo Get(this XmlDocumentation documentation, DocsFieldInfo field)
		{
			if (documentation == null || field == null)
				return null;

			var xmlName = GetXmlName(field);
			return documentation.Members.OfType<XmlDocFieldInfo>().FirstOrDefault(n => n.Name == xmlName);
		}

		public static XmlDocPropertyInfo Get(this XmlDocumentation documentation, DocsPropertyInfo property)
		{
			if (documentation == null || property == null)
				return null;
			
			var xmlName = GetXmlName(property);
			return documentation.Members.OfType<XmlDocPropertyInfo>().FirstOrDefault(n => n.Name == xmlName);
		}

		public static XmlDocMethodInfo Get(this XmlDocTypeInfo type, DocsMethodInfo method)
		{
			if (type == null || method == null)
				return null;

			var xmlName = GetXmlName(method);
			return type.Methods.FirstOrDefault(n => n.Name == xmlName);
		}

		public static XmlDocMethodInfo Get(this XmlDocTypeInfo type, DocsConstructorInfo method)
		{
			if (type == null || method == null)
				return null;

			var xmlName = GetXmlName(method);
			return type.Methods.FirstOrDefault(n => n.Name == xmlName);
		}

		public static XmlDocEventInfo Get(this XmlDocTypeInfo type, DocsEventInfo @event)
		{
			if (type == null || @event == null)
				return null;

			var xmlName = GetXmlName(@event);
			return type.Events.FirstOrDefault(n => n.Name == xmlName);
		}
		
		public static XmlDocFieldInfo Get(this XmlDocTypeInfo type, DocsFieldInfo field)
		{
			if (type == null || field == null)
				return null;

			var xmlName = GetXmlName(field);
			return type.Fields.FirstOrDefault(n => n.Name == xmlName);
		}

		public static XmlDocPropertyInfo Get(this XmlDocTypeInfo type, DocsPropertyInfo property)
		{
			if (type == null || property == null)
				return null;
			
			var xmlName = GetXmlName(property);
			return type.Properties.FirstOrDefault(n => n.Name == xmlName);
		}

		private static string GetXmlName(DocsTypeInfo type)
		{
			// T1: ``0
			if (type.IsGenericMethodParameter)
				return $"``{type.GenericParameterIndex}";

			// T1: `0
			if (type.IsGenericTypeParameter)
				return $"`{type.GenericParameterIndex}";

			// MixedT<T3,T4>: SimpleSample.MixedT{``0,``1}
			if (type.IsGenericType)
				return $"{type.FullName}{{{string.Join(',', type.GenericArguments.Select(GetXmlName))}}}";

			// Type`1 or Type
			return type.IsGenericTypeDefinition
				? $"{type.FullName}`{type.GenericArguments.Length}"
				: type.FullName;
		}
		
		private static string GetXmlName(DocsMethodInfo method)
		{
			// Type.Method``1 or Type.Method
			var baseName = method.GenericArguments != null && method.GenericArguments.Length > 0
				? $"{GetXmlName(method.DeclaringType)}.{method.Name}``{method.GenericArguments.Length}"
				: $"{GetXmlName(method.DeclaringType)}.{method.Name}";

			// Add parameter types
			return method.Parameters.Length == 0
				? baseName
				: $"{baseName}({string.Join(',', method.Parameters.Select(p => GetXmlName(p.ParameterType)))})";
		}
		
		private static string GetXmlName(DocsPropertyInfo property)
		{
			return
				property.IndexParameters == null
					? $"{GetXmlName(property.DeclaringType)}.{property.Name}"
					: $"{GetXmlName(property.DeclaringType)}.{property.Name}({string.Join(',', property.IndexParameters.Select(p => GetXmlName(p.ParameterType)))})";
		}

		private static string GetXmlName(DocsConstructorInfo constructor)
		{
			var baseName = $"{GetXmlName(constructor.DeclaringType)}.#ctor";

			return constructor.Parameters.Length == 0
				? baseName
				: $"{baseName}({string.Join(',', constructor.Parameters.Select(p => GetXmlName(p.ParameterType)))})";
		}

		private static string GetXmlName(DocsMemberInfo property)
		{
			return $"{GetXmlName(property.DeclaringType)}.{property.Name}";
		}
	}
}