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

namespace DocPluck.Reflection
{
	/// <summary>
	/// Discovers the attributes of a parameter and provides access to parameter metadata.
	/// </summary>
	[DataContract]
	public class DocsParameterInfo
	{
		/// <summary>
		/// Gets or sets the attributes of this parameter.
		/// </summary>
		[DataMember]
		public ParameterAttributes Attributes { get; set; }

		/// <summary>
		/// Gets or sets the name of this parameter.
		/// </summary>
		[DataMember]
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the type of this parameter.
		/// </summary>
		[DataMember]
		public DocsTypeInfo ParameterType { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this parameter has a default value.
		/// </summary>
		[DataMember]
		public bool HasDefaultValue { get; set; }

		/// <summary>
		/// Gets or sets the default value of this parameter.
		/// </summary>
		[DataMember]
		public object DefaultValue { get; set; }

		/// <summary>
		/// Gets or sets the custom attributes of this parameter.
		/// </summary>
		[DataMember]
		public DocsCustomAttributeData[] CustomAttributes { get; set; }

		#region Attribute based flags

		/// <inheritdoc cref="ParameterInfo.IsIn" />
		public bool IsIn => (uint) (Attributes & ParameterAttributes.In) > 0U;

		/// <inheritdoc cref="ParameterInfo.IsLcid" />
		public bool IsLcid => (uint) (Attributes & ParameterAttributes.Lcid) > 0U;

		/// <inheritdoc cref="ParameterInfo.IsOptional" />
		public bool IsOptional => (uint) (Attributes & ParameterAttributes.Optional) > 0U;

		/// <inheritdoc cref="ParameterInfo.IsOut" />
		public bool IsOut => (uint) (Attributes & ParameterAttributes.Out) > 0U;

		/// <inheritdoc cref="ParameterInfo.IsRetval" />
		public bool IsRetval => (uint) (Attributes & ParameterAttributes.Retval) > 0U;

		#endregion
	}
}