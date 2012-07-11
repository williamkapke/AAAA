using System;
using System.Reflection;
using AAAA.PropertyExpressions;

namespace AAAA.JsonSerialization
{
	public static class TypeExtensions
	{
		public static ObjectParser GetParserDelegate(this Type type, string name)
		{
			var methodinfo = type.GetMethod(name,
				BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy,
				null,
				new Type[] { typeof(object), typeof(Propex), typeof(object).MakeByRefType() },
				null
			);
			if (methodinfo == null || methodinfo.ReturnType != typeof(bool))
				return null;

			return (ObjectParser)Delegate.CreateDelegate(typeof(ObjectParser), methodinfo);
		}
	}
}
