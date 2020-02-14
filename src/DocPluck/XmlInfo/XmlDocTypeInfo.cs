using System.Collections.Generic;
using System.Runtime.Serialization;
using DocPluck.XmlDocElement;

namespace DocPluck.XmlInfo
{
	public class XmlDocTypeInfo : XmlDocBaseInfo
	{
		public XmlDocTypeInfo(XmlDerivedInfo xmlDerivedInfo) : base(xmlDerivedInfo)
		{
		}
		
		[DataMember]
		public XmlDocNamespaceInfo Namespace { get; set; }
		public Dictionary<string, DocText> TypeParameters => XmlDerivedInfo.TypeParameters;
		[DataMember]
		public List<XmlDocPropertyInfo> Properties { get; } = new List<XmlDocPropertyInfo>();
		[DataMember]
		public List<XmlDocFieldInfo> Fields { get; } = new List<XmlDocFieldInfo>();
		[DataMember]
		public List<XmlDocEventInfo> Events { get; } = new List<XmlDocEventInfo>();
		[DataMember]
		public List<XmlDocMethodInfo> Methods { get; } = new List<XmlDocMethodInfo>();

		public override string Name => $"{XmlDerivedInfo.Name.Namespace}.{XmlDerivedInfo.Name.TypeName}";
	}
}