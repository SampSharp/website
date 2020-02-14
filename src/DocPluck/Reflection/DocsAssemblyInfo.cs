using System.Reflection;
using System.Runtime.Serialization;

namespace DocPluck.Reflection
{
	/// <summary>
	/// Obtains information about the attributes of an assembly and provides access to assembly metadata.
	/// </summary>
	[DataContract]
	public class DocsAssemblyInfo
	{
		/// <summary>
		/// Gets or sets the name of this assembly.
		/// </summary>
		[DataMember]
		public AssemblyName Name { get; set; }

		/// <summary>
		/// Gets or sets the custom attributes of this assembly.
		/// </summary>
		[DataMember]
		public DocsCustomAttributeData[] CustomAttributes { get; set; }

		/// <summary>
		/// Gets the module of this assembly.
		/// </summary>
		[DataMember]
		public DocsModuleInfo Module { get; set; }
	}
}