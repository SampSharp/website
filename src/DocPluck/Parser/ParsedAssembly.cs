using System.Runtime.Serialization;
using DocPluck.Reflection;

namespace DocPluck.Parser
{
	[DataContract]
	public class ParsedAssembly
	{
		public ParsedAssembly(DocsAssemblyInfo assembly, DocsModuleInfo module, DocsNamespaceInfo[] namespaces, DocsTypeInfo[] types)
		{
			Assembly = assembly;
			Module = module;
			Namespaces = namespaces;
			Types = types;
		}
		
		[DataMember]
		public DocsAssemblyInfo Assembly { get; }
		[DataMember]
		public DocsModuleInfo Module { get; }
		[DataMember]
		public DocsNamespaceInfo[] Namespaces { get; }
		[DataMember]
		public DocsTypeInfo[] Types { get; }
	}
}