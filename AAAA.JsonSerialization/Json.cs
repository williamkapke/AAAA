using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AAAA.JsonSerialization
{
	public static class Json
	{
		public static string Write(object obj)
		{
			return null;
		}
		public static string Write(Action<JsonWriter.Object> ow)
		{
			var sw = new System.IO.StringWriter();
			JsonWriter.Write(sw, true, iw =>
			{
				iw.WriteObject(ow);
			}, null);
			return sw.ToString();
		}
	}
}
