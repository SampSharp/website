using System;

namespace SimpleSample
{
//	/// <summary>
//	/// This is some attrib
//	/// </summary>
//	/// <seealso cref="System.Attribute" />
//	[AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
//	public class SomeAttribute : Attribute
//	{
//		/// <summary>
//		/// Initializes a new instance of the <see cref="SomeAttribute"/> class.
//		/// </summary>
//		/// <param name="a">a.</param>
//		/// <param name="b">The b.</param>
//		/// <param name="c">The c.</param>
//		/// <param name="d">The d.</param>
//		/// <param name="e">The e.</param>
//		public SomeAttribute(int a, int b, string c, string d, EnumInt32 e = EnumInt32.A){}
//
//		/// <summary>
//		/// Gets or sets a.
//		/// </summary>
//		public int A { get; set; }
//		/// <summary>
//		/// Gets or sets the b.
//		/// </summary>
//		public int B { get; set; }
//		/// <summary>
//		/// Gets or sets the c.
//		/// </summary>
//		public int C { get; set; }
//		/// <summary>
//		/// Gets or sets the d.
//		/// </summary>
//		public int D { get; set; }
//		/// <summary>
//		/// Gets or sets the e.
//		/// </summary>
//		public int E { get; set; }
//	}
//
//	/// <summary>
//	/// This call is heaviliy attributed
//	/// </summary>
//	[Some(1, 2, "tfoo", null)]
//	[Some(4, 5, "tbar", null, EnumInt32.A)]
//	[Some(4, 5, "tbaz", null, EnumInt32.A, B = 55, C = 88)]
//	public class Attributed
//	{
//		/// <summary>
//		/// Gets or sets the attri property.
//		/// </summary>
//		[Some(1, 2, "pfoo", null)]
//		[Some(4, 5, "pbar", null, EnumInt32.A)]
//		[Some(4, 5, "pbaz", null, EnumInt32.A, B = 55, C = 88)]
//		public int AttriProp { get; set; }
//
//		/// <summary>
//		/// Attributes the method.
//		/// </summary>
//		[Some(1, 2, "mfoo", null)]
//		[Some(4, 5, "mbar", null, EnumInt32.A)]
//		[Some(4, 5, "mbaz", null, EnumInt32.A, B = 55, C = 88)]
//		public void AttribMethod(){}
//
//		/// <summary>
//		/// Occurs when [attribute event].
//		/// </summary>
//		[Some(1, 2, "efoo", null)]
//		[Some(4, 5, "ebar", null, EnumInt32.A)]
//		[Some(4, 5, "ebaz", null, EnumInt32.A, B = 55, C = 88)]
//		public event EventHandler AttribEvent;
//
//		/// <summary>
//		/// The attribute field
//		/// </summary>
//		[Some(1, 2, "ffoo", null)]
//		[Some(4, 5, "fbar", null, EnumInt32.A)]
//		[Some(4, 5, "fbaz", null, EnumInt32.A, B = 55, C = 88)]
//		public int AttribField;
//
//		/// <summary>
//		/// Initializes a new instance of the <see cref="Attributed"/> class.
//		/// </summary>
//		[Some(1, 2, "cfoo", null)]
//		[Some(4, 5, "cbar", null, EnumInt32.A)]
//		[Some(4, 5, "cbaz", null, EnumInt32.A, B = 55, C = 88)]
//		public Attributed()
//		{
//		}
//	}
//
	/// <summary>
	/// an empty interface
	/// </summary>
	public interface IInterface{}
//	/// <summary>
//	/// a simple base class with implements <see cref="IInterface"/>
//	/// </summary>
//	/// <seealso cref="SimpleSample.IInterface" />
//	public class ABase : IInterface{}
//
//	/// <summary>
//	/// a simple sub class of <see cref="ABase"/>
//	/// </summary>
//	/// <seealso cref="SimpleSample.ABase" />
//	public class ASubA : ABase{}
//	/// <summary>
//	/// a simple sub class of ABase
//	/// </summary>
//	/// <seealso cref="SimpleSample.ABase" />
//	public class ASubB : ABase{}
//
//	/// <summary>
//	/// base class with generic with constraint of <see cref="IInterface"/>
//	/// </summary>
//	/// <typeparam name="T"></typeparam>
//	public class BBase<T> where T : IInterface
//	{
//		/// <summary>
//		/// Initializes a new instance of the <see cref="BBase{T}"/> class.
//		/// </summary>
//		public BBase(){}
//		/// <summary>
//		/// Initializes a new instance of the <see cref="BBase{T}"/> class.
//		/// </summary>
//		/// <param name="t">The t.</param>
//		public BBase(T t){}
//		/// <summary>
//		/// Acceptses the t.
//		/// </summary>
//		/// <param name="t">The t.</param>
//		public void AcceptsT(T t){}
//		/// <summary>
//		/// Returnses the t.
//		/// </summary>
//		/// <returns></returns>
//		/// <exception cref="NotImplementedException"></exception>
//		public T ReturnsT(){throw new NotImplementedException();}
//		/// <summary>
//		/// Gets the property of t.
//		/// </summary>
//		/// <value>
//		/// The property of t.
//		/// </value>
//		public T PropOfT { get; }
//		/// <summary>
//		/// The field of t
//		/// </summary>
//		public T FieldOfT;
//	}

	/// <summary>
	/// a class with mixed generics
	/// </summary>
	/// <typeparam name="T1">The type of the 1.</typeparam>
	/// <typeparam name="T2">The type of the 2.</typeparam>
	public class MixedT<T1, T2>
	{
//		/// <summary>
//		/// Methods the t1.
//		/// </summary>
//		/// <returns></returns>
//		/// <exception cref="NotImplementedException"></exception>
//		public T1 MethodT1() => throw new NotImplementedException();
//		/// <summary>
//		/// Methods the t2.
//		/// </summary>
//		/// <returns></returns>
//		/// <exception cref="NotImplementedException"></exception>
//		public T2 MethodT2() => throw new NotImplementedException();
//		/// <summary>
//		/// Methods the t3.
//		/// </summary>
//		/// <typeparam name="T3">The type of the 3.</typeparam>
//		/// <returns></returns>
//		/// <exception cref="NotImplementedException"></exception>
//		public T3 MethodT3<T3>() => throw new NotImplementedException();
//		/// <summary>
//		/// Methods the T2W3.
//		/// </summary>
//		/// <typeparam name="T3">The type of the 3.</typeparam>
//		/// <returns></returns>
//		/// <exception cref="NotImplementedException"></exception>
//		public T2 MethodT2w3<T3>() => throw new NotImplementedException();

		/// <summary>
		/// Methods the T2W3W4.
		/// </summary>
		/// <typeparam name="T3">The type of the 3.</typeparam>
		/// <typeparam name="T4">The type of the 4.</typeparam>
		/// <param name="a">a.</param>
		/// <param name="b">The b.</param>
		/// <param name="c">The c.</param>
		/// <returns></returns>
		/// <exception cref="NotImplementedException"></exception>
		public T2 MethodT2w3w4<T3, T4>(T3 a, T4 b, MixedT<T3, T4> c) where T3 : IInterface where T4 : MixedT<T3, T4>
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Gets the <see cref="T1"/> with the specified a.
		/// </summary>
		/// <value>
		/// The <see cref="T1"/>.
		/// </value>
		/// <param name="a">a.</param>
		/// <param name="b">The b.</param>
		/// <returns></returns>
		public T1 this[int a, T2 b]
		{
			get => default;
			set{}
		}

		/// <summary>
		/// Sets the <see cref="T1"/> with the specified a.
		/// </summary>
		/// <value>
		/// The <see cref="T1"/>.
		/// </value>
		/// <param name="a">a.</param>
		/// <param name="z">The z.</param>
		/// <param name="b">The b.</param>
		/// <returns></returns>
		public T1 this[int a, int z, T2 b]
		{
			set{}
		}
	}
//	/// <summary>
//	/// subclass of generic class
//	/// </summary>
//	/// <seealso cref="BBase{ASubA}" />
//	public class BSubA : BBase<ASubA>{}
//
//	/// <summary>
//	/// base class T with a default constructor
//	/// </summary>
//	/// <typeparam name="T">the type</typeparam>
//	public class CBaseDefCon<T> where T : IInterface, new() {}
//
//	/// <summary>
//	/// base class T with ref type
//	/// </summary>
//	/// <typeparam name="T">type the</typeparam>
//	public class CBaseRefType<T> where T : class, IInterface {}
//
//	/// <summary>
//	/// base class T with non null
//	/// </summary>
//	/// <typeparam name="T">the type the</typeparam>
//	public class CBaseNonNull<T> where T : struct, IInterface {}
//
//	/// <summary>
//	/// enum32
//	/// </summary>
//	public enum EnumInt32 : int
//	{
//		/// <summary>
//		/// a
//		/// </summary>
//		A
//	}
//
//	/// <summary>
//	/// enum u32
//	/// </summary>
//	public enum EnumUInt32 : uint
//	{
//		/// <summary>
//		/// a
//		/// </summary>
//		A
//	}
//
//	/// <summary>
//	/// enum int 16
//	/// </summary>
//	public enum EnumInt16 : Int16
//	{
//		/// <summary>
//		/// a
//		/// </summary>
//		A
//	}
//
//	/// <summary>
//	/// enum u 16
//	/// </summary>
//	public enum EnumUInt16 : UInt16
//	{
//		/// <summary>
//		/// a
//		/// </summary>
//		A
//	}
//
//	/// <summary>
//	/// enum 8
//	/// </summary>
//	public enum EnumInt8 : sbyte
//	{
//		/// <summary>
//		/// a
//		/// </summary>
//		A
//	}
//
//	/// <summary>
//	/// enum u8
//	/// </summary>
//
//	public enum EnumUInt8 : byte
//	{
//		/// <summary>
//		/// a
//		/// </summary>
//		A
//	}
//
//	/// <summary>
//	/// void del
//	/// </summary>
//	public delegate void VoidDelegate();
//
//	/// <summary>
//	/// int del
//	/// </summary>
//	/// <returns></returns>
//	public delegate int IntDelegate();
//
//	/// <summary>
//	/// int del w args
//	/// </summary>
//	/// <param name="a">a.</param>
//	/// <param name="b">The b.</param>
//	/// <param name="c">The c.</param>
//	/// <returns></returns>
//	public delegate int IntDelegateWithArgs(int a, int b, int c);
//
//	/// <summary>
//	/// Sample class 1.
//	/// </summary>
//	/// <remarks>Class 1 remarks.</remarks>
//	/// <example>
//	///	<code>new Class1()</code>
//	/// </example>
//	public class Class1
//	{
//		/// <summary>
//		/// publi
//		/// </summary>
//		public class PublicClass {}
//		/// <summary>
//		/// privpriv
//		/// </summary>
//		private class PrivateClass {}
//		/// <summary>
//		/// protec
//		/// </summary>
//		protected class ProtectedClass {}
//		/// <summary>
//		/// inter
//		/// </summary>
//		internal class InternalClass {}
//		/// <summary>
//		/// pprot inter
//		/// </summary>
//		protected internal class ProtectedInternalClass {}
//		/// <summary>
//		/// private prot
//		/// </summary>
//		private protected class PrivateProtectedClass {}
//		
//		#region Property protection level tests
//
//		//public int A11 { public get; set; }
//		public int A12 { internal get; set; }
//		public int A13 { private get; set; }
//		public int A14 { protected get; set; }
//		public int A15 { protected internal get; set; }
//		public int A16 { private protected get; set; }
//		
//		//protected internal int A51 { public get; set; }
//		protected internal int A52 { internal get; set; }
//		protected internal int A53 { private get; set; }
//		protected internal int A54 { protected get; set; }
//		//protected internal int A55 { protected internal get; set; }
//		protected internal int A56 { private protected get; set; }
//		
//		//protected int A41 { public get; set; }
//		//protected int A42 { internal get; set; }
//		protected int A43 { private get; set; }
//		//protected int A44 { protected get; set; }
//		//protected int A45 { protected internal get; set; }
//		protected int A46 { private protected get; set; }
//		// same v & ^
//		//internal int A21 { public get; set; }
//		//internal int A22 { internal get; set; }
//		internal int A23 { private get; set; }
//		//internal int A24 { protected get; set; }
//		//internal int A25 { protected internal get; set; }
//		internal int A26 { private protected get; set; }
//		
//		//private protected int A61 { public get; set; }
//		//private protected int A62 { internal get; set; }
//		private protected int A63 { private get; set; }
//		//private protected int A64 { protected get; set; }
//		//private protected int A65 { protected internal get; set; }
//		//private protected int A66 { private protected get; set; }
//
//		//private int A31 { public get; set; }
//		//private int A32 { internal get; set; }
//		//private int A33 { private get; set; }
//		//private int A34 { protected get; set; }
//		//private int A35 { protected internal get; set; }
//		//private int A36 { private protected get; set; }
//
//		#endregion
//
//		/// <summary>
//		/// Occurs when [foo].
//		/// </summary>
//		/// <exception cref="NotImplementedException">
//		/// </exception>
//		public event EventHandler Foo
//		{
//			add
//			{
//				throw new NotImplementedException();
//			}
//			remove
//			{
//				throw new NotImplementedException();
//			}
//		}
//
//		/// <summary>
//		/// Occurs when [bar].
//		/// </summary>
//		public event EventHandler Bar;
//
//		/// <summary>
//		/// Occurs when [add only event].
//		/// </summary>
//		public event EventHandler AddOnlyEvent
//		{
//			add
//			{
//
//			}
//			remove
//			{
//
//			}
//		}
//
//		/// <summary>
//		/// A method with a void return value
//		/// </summary>
//		public void MethodWithVoidReturnType()
//		{
//
//		}
//
//		
//		/// <summary>
//		/// <paramref name="a"/> is a param of this method
//		/// </summary>
//		/// <param name="a">a.</param>
//		public void MethodStartsWithParamRef(int a)
//		{
//
//		}
//		/// <summary>
//		/// there is a param named <paramref name="a"/>
//		/// </summary>
//		/// <param name="a">a.</param>
//		public void MethodEndsWithParamRef(int a)
//		{
//
//		}
//
//		/// <summary>
//		/// <paramref name="a"/> is a param and so is <paramref name="b"/>
//		/// </summary>
//		/// <param name="a">a.</param>
//		/// <param name="b">b.</param>
//		public void MethodSurroundedWithParamRef(int a, int b)
//		{
//
//		}
//
//		/// <summary>
//		/// Methods the with default values.
//		/// </summary>
//		/// <param name="a">a.</param>
//		/// <param name="b">The b.</param>
//		/// <param name="x">The x.</param>
//		/// <param name="jz">The jz.</param>
//		/// <param name="z">The z.</param>
//		/// <param name="q">The q.</param>
//		public void MethodWithDefaultValues(int a = 1, int b = 6666, float x = 0.4525f, double jz = 0.242141, object z = null, EnumInt16 q = default){}
//	}
}
