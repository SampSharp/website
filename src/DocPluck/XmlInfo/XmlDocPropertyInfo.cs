using System.Collections.Generic;
using System.Runtime.Serialization;
using DocPluck.XmlDocElement;

namespace DocPluck.XmlInfo
{
	public class XmlDocPropertyInfo : XmlDocMemberInfo
	{
		public XmlDocPropertyInfo(XmlDerivedInfo xmlDerivedInfo) : base(xmlDerivedInfo)
		{
		}
		
		[DataMember]
		public Dictionary<string, DocText> Parameters { get; set; }
		[DataMember]
		public Dictionary<CodeReference, DocText> Exceptions { get; set; }
		[DataMember]
		public DocText Value { get; set; }

		public override string Name => $"{XmlDerivedInfo.Name.Namespace}.{XmlDerivedInfo.Name.TypeName}.{XmlDerivedInfo.Name.MemberName}";
	}
}