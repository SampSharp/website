using System.Runtime.Serialization;

namespace DocPluck.XmlInfo
{
	public abstract class XmlDocMemberInfo : XmlDocBaseInfo
	{
		protected XmlDocMemberInfo(XmlDerivedInfo xmlDerivedInfo) : base(xmlDerivedInfo)
		{
		}
		
		[DataMember]
		public XmlDocTypeInfo DeclaringType { get; set; }
	}
}