using System.Collections.Generic;
using DocPluck.XmlDocElement;

namespace DocPluck.XmlInfo
{
	public class XmlDocMethodInfo : XmlDocMemberInfo
	{
		public XmlDocMethodInfo(XmlDerivedInfo xmlDerivedInfo) : base(xmlDerivedInfo)
		{
		}

		public Dictionary<string, DocText> Parameters => XmlDerivedInfo.Parameters;
		public Dictionary<string, DocText> TypeParameters => XmlDerivedInfo.TypeParameters;
		public DocText Returns => XmlDerivedInfo.Returns;
		public Dictionary<CodeReference, DocText> Exceptions => XmlDerivedInfo.Exceptions;

		public override string Name => $"{XmlDerivedInfo.Name.Namespace}.{XmlDerivedInfo.Name.TypeName}.{XmlDerivedInfo.Name.MemberName}";
	}
}