using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DocPluck.XmlInfo
{
	public class XmlDocNamespaceInfo : XmlDocBaseInfo
	{
		public XmlDocNamespaceInfo(XmlDerivedInfo xmlDerivedInfo) : base(xmlDerivedInfo)
		{
		}
		
		[DataMember]
		public List<XmlDocTypeInfo> Types { get; } = new List<XmlDocTypeInfo>();

		public override string Name => XmlDerivedInfo.Name.Namespace;
	}
}