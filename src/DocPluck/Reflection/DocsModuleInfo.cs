using System.Runtime.Serialization;

namespace DocPluck.Reflection
{
	/// <summary>
	/// Obtains information about the attributes of a module and provides access to module metadata.
	/// </summary>
	[DataContract]
	public class DocsModuleInfo
	{
		/// <summary>
		/// Gets or sets the name of this module.
		/// </summary>
		[DataMember]
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the custom attributes of this module.
		/// </summary>
		[DataMember]
		public DocsCustomAttributeData[] CustomAttributes { get; set; }

		/// <summary>
		/// Gets or sets the assembly of this module.
		/// </summary>
		[DataMember]
		public DocsAssemblyInfo Assembly { get; set; }
	}
}