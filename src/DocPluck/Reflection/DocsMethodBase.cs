using System.Reflection;
using System.Runtime.Serialization;
using DocPluck.XmlInfo;

namespace DocPluck.Reflection
{
	/// <summary>
	/// Provides information about methods and constructors.
	/// </summary>
	public abstract class DocsMethodBase : DocsMemberInfo
	{
		/// <summary>
		/// Gets or sets the parameters of this method.
		/// </summary>
		[DataMember]
		public DocsParameterInfo[] Parameters { get; set; }

		/// <summary>
		/// Gets or sets the attributes of this method.
		/// </summary>
		[DataMember]
		public MethodAttributes Attributes { get; set; }

		/// <summary>
		/// Gets or sets the generic arguments of this method.
		/// </summary>
		[DataMember]
		public DocsTypeInfo[] GenericArguments { get; set; }

		#region Attribute based flags

		/// <inheritdoc cref="MethodBase.IsAbstract"/>
		public bool IsAbstract => (uint) (this.Attributes & MethodAttributes.Abstract) > 0U;
		
		/// <inheritdoc cref="MethodBase.IsConstructor"/>
		public bool IsConstructor => this is DocsConstructorInfo && !IsStatic && (Attributes & MethodAttributes.RTSpecialName) == MethodAttributes.RTSpecialName;
		
		/// <inheritdoc cref="MethodBase.IsFinal"/>
		public bool IsFinal => (uint) (this.Attributes & MethodAttributes.Final) > 0U;
		
		/// <inheritdoc cref="MethodBase.IsHideBySig"/>
		public bool IsHideBySig => (uint) (this.Attributes & MethodAttributes.HideBySig) > 0U;
		
		/// <inheritdoc cref="MethodBase.IsSpecialName"/>
		public bool IsSpecialName => (uint) (this.Attributes & MethodAttributes.SpecialName) > 0U;
		
		/// <inheritdoc cref="MethodBase.IsStatic"/>
		public bool IsStatic => (uint) (this.Attributes & MethodAttributes.Static) > 0U;
		
		/// <inheritdoc cref="MethodBase.IsVirtual"/>
		public bool IsVirtual => (uint) (this.Attributes & MethodAttributes.Virtual) > 0U;
		
		/// <inheritdoc cref="MethodBase.IsAssembly"/>
		public bool IsAssembly => (this.Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.Assembly;
		
		/// <inheritdoc cref="MethodBase.IsFamily"/>
		public bool IsFamily => (this.Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.Family;
		
		/// <inheritdoc cref="MethodBase.IsFamilyAndAssembly"/>
		public bool IsFamilyAndAssembly => (this.Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.FamANDAssem;
		
		/// <inheritdoc cref="MethodBase.IsFamilyOrAssembly"/>
		public bool IsFamilyOrAssembly => (this.Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.FamORAssem;
		
		/// <inheritdoc cref="MethodBase.IsPrivate"/>
		public bool IsPrivate => (this.Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.Private;
		
		/// <inheritdoc cref="MethodBase.IsPublic"/>
		public bool IsPublic => (this.Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.Public;

		#endregion
		
		/// <summary>
		/// Gets the documentation of this method.
		/// </summary>
		[DataMember]
		public XmlDocMethodInfo Documentation { get; set; }

		public override DocsAccessibilityLevel AccessibilityLevel =>
			IsPublic ? DocsAccessibilityLevel.Public :
			IsAssembly ? DocsAccessibilityLevel.Internal :
			IsPrivate ? DocsAccessibilityLevel.Private :
			IsFamily ? DocsAccessibilityLevel.Protected :
			IsFamilyAndAssembly ? DocsAccessibilityLevel.PrivateProtected :
			IsFamilyOrAssembly ? DocsAccessibilityLevel.ProtectedInternal : DocsAccessibilityLevel.Unknown;

	}
}