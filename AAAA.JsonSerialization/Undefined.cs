using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace AAAA.JsonSerialization
{
	[DebuggerDisplay("undefined")]
	public class Undefined
	{
		public static readonly Undefined Value = new Undefined();
		private Undefined() { }
		public override string ToString()
		{
			throw new InvalidOperationException("Undefined does not have a value");
		}
	}
}
