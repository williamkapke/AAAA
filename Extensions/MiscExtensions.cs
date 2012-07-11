using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Reflection;
using System.Xml.Linq;
using System.Web;

namespace AAAA
{
	public static class MiscExtensions
	{
		public static object Create(this Type type, Type[] genericTypeArgs, params object[] args)
		{
			var genericType = type.MakeGenericType(genericTypeArgs);
			return Activator.CreateInstance(genericType, args);
		}
		public static void Append(this StringBuilder sb, string value1, string value2, string value3)
		{
			sb.Append(value1).Append(value2).Append(value3);
		}
		public static void Append(this StringBuilder sb, string value1, string value2, string value3, string value4)
		{
			sb.Append(value1).Append(value2).Append(value3).Append(value4);
		}
		public static void Append(this StringBuilder sb, string value1, string value2, string value3, string value4, string value5)
		{
			sb.Append(value1).Append(value2).Append(value3).Append(value4).Append(value5);
		}

		public static string ToUrlString(this Dictionary<string, string> items)
		{
			if (items == null) return "";
			var length = items.Count;
			if (length == 0) return "";

			bool first = true;
			var sb = new StringBuilder();
			foreach (var kvp in items)
			{
				if (!first) sb.Append('&');
				first = false;
				sb.Append(HttpUtility.UrlEncode(kvp.Key)).Append('=').Append(HttpUtility.UrlEncode(kvp.Value));
			}
			return sb.ToString();
		}
	}
}
