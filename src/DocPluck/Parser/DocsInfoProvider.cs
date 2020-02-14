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
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using DocPluck.Reflection;

namespace DocPluck.Parser
{
	internal class DocsInfoProvider
	{
		private readonly MetadataReader _reader;
		private readonly Dictionary<EntityHandle, DocsTypeInfo> _types = new Dictionary<EntityHandle, DocsTypeInfo>();
		private readonly Dictionary<EntityHandle, DocsTypeInfo> _typeReferences = new Dictionary<EntityHandle, DocsTypeInfo>();
		private readonly Dictionary<EntityHandle, DocsAssemblyInfo> _asmReferences = new Dictionary<EntityHandle, DocsAssemblyInfo>();
		private readonly Dictionary<NamespaceDefinitionHandle, DocsNamespaceInfo> _namespaces = new Dictionary<NamespaceDefinitionHandle, DocsNamespaceInfo>();

		public DocsInfoProvider(MetadataReader reader)
		{
			_reader = reader;
		}

		public IEnumerable<DocsTypeInfo> Types => _types.Values;
		
		public IEnumerable<DocsNamespaceInfo> Namespaces => _namespaces.Values;

		public DocsTypeInfo ResolveType(EntityHandle handle)
		{
			if (handle.IsNil)
				return null;

			DocsTypeInfo value;

			switch (handle.Kind)
			{
				case HandleKind.ExportedType:
					var exportedType = _reader.GetExportedType((ExportedTypeHandle) handle);
					if (exportedType.Implementation.IsNil)
					{
						Console.WriteLine("NIL?");
					}

					return ResolveType(exportedType.Implementation);
				case HandleKind.TypeDefinition:
					if (_types.TryGetValue(handle, out value))
						return value;

					return _types[handle] = new DocsTypeInfo();
				case HandleKind.TypeReference:
					if (_typeReferences.TryGetValue(handle, out value))
						return value;

					var typeReference = _reader.GetTypeReference((TypeReferenceHandle) handle);

					if (!_asmReferences.TryGetValue(typeReference.ResolutionScope, out var assembly) &&
					    typeReference.ResolutionScope.Kind == HandleKind.AssemblyReference)
					{
						var asmRef = _reader.GetAssemblyReference((AssemblyReferenceHandle) typeReference.ResolutionScope);
						assembly = new DocsAssemblyInfo
						{
							Name = asmRef.GetAssemblyName()
						};

						_asmReferences[typeReference.ResolutionScope] = assembly;
					}

					return _typeReferences[handle] = new DocsTypeInfo
					{
						Namespace = _reader.GetString(typeReference.Namespace),
						Name = _reader.GetString(typeReference.Name),
						Assembly = assembly,
						IsExternalReference = true
					};
				case HandleKind.TypeSpecification:
					var typeSpecification = _reader.GetTypeSpecification((TypeSpecificationHandle) handle);
					
					var blobReader = _reader.GetBlobReader(typeSpecification.Signature);
					var tmp = new SignatureTypeProvider(this);
					var deco = new SignatureDecoder<DocsTypeInfo, object>(tmp, _reader, null).DecodeType(ref blobReader);
					return deco;
				default:
					throw new ArgumentOutOfRangeException(nameof(handle));
			}
		}
		
		public DocsNamespaceInfo ResolveNamespace(NamespaceDefinitionHandle handle)
		{
			if (handle.IsNil)
				return null;

			if (_namespaces.TryGetValue(handle, out var value))
				return value;

			return _namespaces[handle] = new DocsNamespaceInfo();
		}
	}
}