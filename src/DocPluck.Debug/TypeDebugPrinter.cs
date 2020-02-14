using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using DocPluck.Reflection;

namespace DocPluck.Debug
{
	internal static class TypeDebugPrinter
	{
		private static string GetTypeDisplayName(DocsTypeInfo type)
		{
			if (type.IsGenericParameter)
			{
				return type.Name;
			}

			var name = type.FullName;

			static string Flags(DocsTypeInfo t) =>
				(t.GenericParameterAttributes & GenericParameterAttributes.VarianceMask) switch
				{
					GenericParameterAttributes.Covariant => "out ",
					GenericParameterAttributes.Contravariant => "in ",
					_ => null
				};

			if (type.IsGenericTypeDefinition)
			{
				return name + $"<{string.Join(", ", type.GenericArguments.Select(g => Flags(g) + g.Name))}>";
			}
			
			if (type.IsGenericType)
			{
				return name + $"<{string.Join(", ", type.GenericArguments.Select(GetTypeDisplayName))}>";
			}
			return name;
		}

		private static void PrintAttributes(DocsMemberInfo member, string pfx = null)
		{
			foreach (var attr in member.CustomAttributes)
			{
				Console.Write(pfx);
				Console.Write("[");

				Console.Write(GetTypeDisplayName(attr.AttributeType));

				if (attr.FixedArguments.Length > 0 || attr.NamedArguments.Length > 0)
				{
					Console.Write("(");

					Console.Write(string.Join(", ", attr.FixedArguments.Select(a => a.Value ?? "null")));

					if (attr.FixedArguments.Length > 0 && attr.NamedArguments.Length > 0)
						Console.Write(", ");

					Console.Write(string.Join(", ", attr.NamedArguments.Select(a => a.Name + " = " + (a.Value ?? "null"))));

					Console.Write(")");
				}

				Console.WriteLine("]");
			}
		}

		private static string GetParametersString(DocsParameterInfo[] parameters)
		{
			return string.Join(", ", parameters.Select(p =>
			{
				var sb = new StringBuilder();

				if (p.IsOut)
					sb.Append("out ");

				if (p.IsIn)
					sb.Append("in ");

				sb.Append(GetTypeDisplayName(p.ParameterType));
				sb.Append(" ");
				sb.Append(p.Name);
						
				if (p.HasDefaultValue)
				{
					sb.Append(" = ");
					sb.Append(p.DefaultValue ?? "null");
				}
				return sb.ToString();
			}));
		}

		public static void PrintTypes(IEnumerable<DocsTypeInfo> types)
		{
			foreach (var type in types)
			{
				Console.WriteLine("/// " + type.Documentation?.Summary);
				PrintAttributes(type);

				var typeModifiers = DocsAccessibilityHelper.ToString(type.AccessibilityLevel);

				if (type.IsDelegate)
				{
					var invoke = type.Methods.First(m => m.Name == "Invoke");

					Console.Write(typeModifiers + " " + GetTypeDisplayName(invoke.ReturnType) + " " + GetTypeDisplayName(type));

					Console.WriteLine($"({GetParametersString(invoke.Parameters)});");
					continue;
				}
				
				if (type.IsAbstract)
					typeModifiers += " abstract";

				Console.Write(typeModifiers + " " + GetTypeDisplayName(type));

				if (type.BaseType != null || type.InterfaceImplementations.Count > 0) Console.Write(" : ");

				if (type.BaseType != null)
				{
					Console.Write(GetTypeDisplayName(type.BaseType));

					if (type.InterfaceImplementations.Count > 0)
						Console.Write(", ");
				}

				Console.Write(string.Join(", ", type.InterfaceImplementations.Select(t => GetTypeDisplayName(t))));

				if (type.IsGenericTypeDefinition)
				{
					var isFirst = true;
					foreach (var genericArgument in type.GenericArguments)
					{
						var started = false;

						void Separator()
						{
							if (started)
							{
								Console.Write(", ");
								return;
							}
							
							started = true;

							if (isFirst)
								isFirst = false;
							else
								Console.Write(", ");

							Console.Write(" where " + genericArgument.Name + " : ");
						}

						var specialConstraint = (genericArgument.GenericParameterAttributes & GenericParameterAttributes.SpecialConstraintMask);

						switch (specialConstraint)
						{
							case GenericParameterAttributes.ReferenceTypeConstraint:
								Separator();
								Console.Write("class");
								break;
							case GenericParameterAttributes.NotNullableValueTypeConstraint:
								Separator();
								Console.Write("struct");
								break;
						}

						foreach (var con in genericArgument.GenericParameterConstraints)
						{
							Separator();
							Console.Write(GetTypeDisplayName(con));
						}

						// new() comes last
						if (specialConstraint== GenericParameterAttributes.DefaultConstructorConstraint)
						{
							Separator();
							Console.Write("new()");
						}
					}
				}

				Console.WriteLine();

				foreach (var constructor in type.Constructors)
				{
					Console.WriteLine("  /// " + constructor.Documentation?.Summary);
					var modifiers = DocsAccessibilityHelper.ToString(constructor.AccessibilityLevel);

					PrintAttributes(constructor, "  ");
					Console.WriteLine($"  {modifiers} {type.Name}({GetParametersString(constructor.Parameters)})");
				}

				foreach (var method in type.Methods)
				{
					Console.WriteLine("  /// " + method.Documentation?.Summary);
					var modifiers = DocsAccessibilityHelper.ToString(method.AccessibilityLevel);

					if (method.IsAbstract)
						modifiers += " abstract";

					if (method.IsStatic)
						modifiers += " static";

					PrintAttributes(method, "  ");
					Console.Write($"  {modifiers} {GetTypeDisplayName(method.ReturnType)} {method.Name}");
					
					static string Flags(DocsTypeInfo t) =>
						(t.GenericParameterAttributes & GenericParameterAttributes.VarianceMask) switch
						{
							GenericParameterAttributes.Covariant => "out ",
							GenericParameterAttributes.Contravariant => "in ",
							_ => null
						};

					if (method.GenericArguments != null)
					{
						Console.Write($"<{string.Join(", ", method.GenericArguments.Select(g => Flags(g) + g.Name))}>");
					}

					Console.Write($"({GetParametersString(method.Parameters)})");
	
					if (method.GenericArguments != null)
					{
						var isFirst = true;
						foreach (var genericArgument in method.GenericArguments)
						{
							var started = false;

							void Separator()
							{
								if (started)
								{
									Console.Write(", ");
									return;
								}
							
								started = true;

								if (isFirst)
									isFirst = false;
								else
									Console.Write(", ");

								Console.Write(" where " + genericArgument.Name + " : ");
							}

							var specialConstraint = (genericArgument.GenericParameterAttributes & GenericParameterAttributes.SpecialConstraintMask);

							switch (specialConstraint)
							{
								case GenericParameterAttributes.ReferenceTypeConstraint:
									Separator();
									Console.Write("class");
									break;
								case GenericParameterAttributes.NotNullableValueTypeConstraint:
									Separator();
									Console.Write("struct");
									break;
							}

							foreach (var con in genericArgument.GenericParameterConstraints)
							{
								Separator();
								Console.Write(GetTypeDisplayName(con));
							}

							// new() comes last
							if (specialConstraint == GenericParameterAttributes.DefaultConstructorConstraint)
							{
								Separator();
								Console.Write("new()");
							}
						}
					}


					Console.WriteLine();
				}

				foreach (var field in type.Fields)
				{
					Console.WriteLine("  /// " + field.Documentation?.Summary);
					var modifiers = DocsAccessibilityHelper.ToString(field.AccessibilityLevel);

					if (field.IsStatic)
						modifiers += " static";
					
					PrintAttributes(field, "  ");
					Console.WriteLine($"  {modifiers} {GetTypeDisplayName(field.FieldType)} {field.Name}{(field.EnumValue != null ? " = " + field.EnumValue : null)};");
				}

				foreach (var @event in type.Events)
				{
					Console.WriteLine("  /// " + @event.Documentation?.Summary);
					var modifiers = DocsAccessibilityHelper.ToString(@event.AccessibilityLevel);
					
					PrintAttributes(@event, "  ");
					Console.WriteLine($"  {modifiers} event {GetTypeDisplayName(@event.EventHandlerType)} {@event.Name};");
				}

				foreach (var property in type.Properties)
				{
					Console.WriteLine("  /// " + property.Documentation?.Summary);
					var modifiers = DocsAccessibilityHelper.ToString(property.AccessibilityLevel);

					var methods = "";
					if (property.CanRead)
						methods += $" {(property.GetMethod.AccessibilityLevel == property.AccessibilityLevel ? null : DocsAccessibilityHelper.ToString(property.GetMethod.AccessibilityLevel) + " ")}get;";
					if (property.CanWrite)
						methods += $" {(property.SetMethod.AccessibilityLevel == property.AccessibilityLevel ? null : DocsAccessibilityHelper.ToString(property.SetMethod.AccessibilityLevel) + " ")}set;";
					
					PrintAttributes(property, "  ");

					Console.WriteLine(property.IndexParameters != null
						? $"  {modifiers} {GetTypeDisplayName(property.PropertyType)} this [{GetParametersString(property.IndexParameters)}] {{{methods} }}"
						: $"  {modifiers} {GetTypeDisplayName(property.PropertyType)} {property.Name} {{{methods} }}");
				}

				Console.WriteLine();
			}
		}
	}
}