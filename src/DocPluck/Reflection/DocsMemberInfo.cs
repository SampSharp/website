using System.Runtime.Serialization;

namespace DocPluck.Reflection
{
	/// <summary>
	/// Obtains information about the attributes of a member and provides access to member metadata.
	/// </summary>
	[DataContract]
	public abstract class DocsMemberInfo
	{
		[DataMember]
		private string _name;

		/// <summary>
		/// Gets or sets the declaring type of this member.
		/// </summary>
		[DataMember]
		public DocsTypeInfo DeclaringType { get; set; }

		/// <summary>
		/// Gets or sets the name of this member.
		/// </summary>
		public virtual string Name
		{
			get => _name;
			set => _name = value;
		}

		/// <summary>
		/// Gets the accessibility level of this member.
		/// </summary>
		public virtual DocsAccessibilityLevel AccessibilityLevel => DocsAccessibilityLevel.Unknown;

		/// <summary>
		/// Gets or sets the custom attributes of this member.
		/// </summary>
		[DataMember]
		public DocsCustomAttributeData[] CustomAttributes { get; set; }
	}
}