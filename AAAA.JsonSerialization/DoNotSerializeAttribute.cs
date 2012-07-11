using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AAAA.JsonSerialization
{
	[Flags]
	public enum SerializeDirection
	{
		None=0, In=1, Out=2, InOut=In|Out
	}
	public interface IDoNotSerialize
	{
		SerializeDirection Direction { get; }
	}
	/// <summary>
	///		Prevents serialization of the property.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public sealed class DoNotSerializeAttribute : Attribute, IDoNotSerialize
	{
		public SerializeDirection Direction { get { return SerializeDirection.InOut; } }
	}
	public class DoNotSerialize
	{
		private DoNotSerialize() { }

		/// <summary>
		///		Prevents inbound serialization of the property.
		/// </summary>
		[AttributeUsage(AttributeTargets.Property)]
		public sealed class InAttribute : Attribute, IDoNotSerialize
		{
			public SerializeDirection Direction { get { return SerializeDirection.In; } }
		}
		/// <summary>
		///		Prevents outbound serialization of the property.
		/// </summary>
		[AttributeUsage(AttributeTargets.Property)]
		public sealed class OutAttribute : Attribute, IDoNotSerialize
		{
			public SerializeDirection Direction { get { return SerializeDirection.Out; } }
		}
	}
}
