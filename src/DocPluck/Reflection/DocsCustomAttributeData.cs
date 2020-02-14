using System.Runtime.Serialization;

namespace DocPluck.Reflection
{
	/// <summary>
	/// Provides access to custom attribute data for types, members and parameters that are loaded into the reflection-only context.
	/// </summary>
	[DataContract]
	public class DocsCustomAttributeData
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DocsCustomAttributeData"/> class.
		/// </summary>
		/// <param name="attributeType">Type of the custom attribute.</param>
		/// <param name="fixedArguments">The fixed arguments of the custom attribute.</param>
		/// <param name="namedArguments">The named arguments of the custom attribute.</param>
		public DocsCustomAttributeData(DocsTypeInfo attributeType, DocsCustomAttributeArgument[] fixedArguments, DocsCustomAttributeNamedArgument[] namedArguments)
		{
			AttributeType = attributeType;
			FixedArguments = fixedArguments;
			NamedArguments = namedArguments;
		}

		/// <summary>
		/// Gets the type of the custom attribute.
		/// </summary>
		[DataMember]
		public DocsTypeInfo AttributeType { get; }
		/// <summary>
		/// Gets the fixed arguments of the custom attribute.
		/// </summary>
		[DataMember]
		public DocsCustomAttributeArgument[] FixedArguments { get; }
		/// <summary>
		/// Gets the named arguments of the custom attribute.
		/// </summary>
		[DataMember]
		public DocsCustomAttributeNamedArgument[] NamedArguments { get; }
	}
}