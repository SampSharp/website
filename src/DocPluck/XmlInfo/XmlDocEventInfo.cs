namespace DocPluck.XmlInfo
{
	public class XmlDocEventInfo : XmlDocMemberInfo
	{
		public XmlDocEventInfo(XmlDerivedInfo xmlDerivedInfo) : base(xmlDerivedInfo)
		{
		}

		public override string Name => $"{XmlDerivedInfo.Name.Namespace}.{XmlDerivedInfo.Name.TypeName}.{XmlDerivedInfo.Name.MemberName}";
	}
}