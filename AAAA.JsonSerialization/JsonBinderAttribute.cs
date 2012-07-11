using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AAAA.PropertyExpressions;

namespace AAAA.JsonSerialization
{
	[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
	public class JsonBinderAttribute : Attribute
	{
		public JsonBinderAttribute(Type type, string targets)
		{
			Type = type;
			Targets = targets;
		}
		public Type Type { get; private set; }
		public Propex Targets { get; private set; }
	}
}
