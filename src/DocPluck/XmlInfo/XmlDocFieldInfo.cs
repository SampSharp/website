namespace DocPluck.XmlInfo
{
	public class XmlDocFieldInfo : XmlDocMemberInfo
	{
		public XmlDocFieldInfo(XmlDerivedInfo xmlDerivedInfo) : base(xmlDerivedInfo)
		{
		}

		public override string Name => $"{XmlDerivedInfo.Name.Namespace}.{XmlDerivedInfo.Name.TypeName}.{XmlDerivedInfo.Name.MemberName}";
	}
}