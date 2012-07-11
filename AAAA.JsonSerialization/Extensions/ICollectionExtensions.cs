using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using AAAA.PropertyExpressions;

namespace AAAA.JsonSerialization
{
	public static class ICollectionExtensions
	{
		public static string ToJson(this ICollection items, bool formatted = false, char formatChar = '\t', int maxDepth = JsonWriter.MAX_DEPTH, bool ignoreDirectionRestrictions = false, Propex targets = null)
		{
			JsonWriterOptions options = new JsonWriterOptions(maxDepth, formatChar, formatted, ignoreDirectionRestrictions, targets);
			return items.ToJson(options);
		}
		public static string ToJson(this ICollection items, JsonWriterOptions options)
		{
			var sw = new System.IO.StringWriter();
			JsonWriter.Write(sw, true, iw => iw.WriteArray(items.Write), options);
			return sw.ToString();
		}
		public static void Write(this ICollection items, JsonWriter.Array aw)
		{
			var targets = aw.ParentProperty.SubProperties;
			if (targets != null && !targets.IsArray)
				throw new InvalidOperationException("Property's SubProperties must be an array.");

			if (targets == null)// || !targets.HasIndexProperties)
			{
				int i = 0;
				foreach (var item in (IEnumerable)items)
				{
					aw.WriteValue(new Property(i.ToString(), false, null), item);
					i++;
				}
			}
			else
			{
				//convert to IList so we can get the values by index
				var list = items as IList;
				if (list == null)
					list = new ArrayList(items);

				var defaultProperty = targets["-1"];

				int max = Math.Min(targets.Max, list.Count - 1);
				for (int i = targets.Min; i <= max; i++)
				{
					var prop = targets[i.ToString()] ?? defaultProperty;
					if (prop != null)
						aw.WriteValue(prop, list[i]);
				}
			}
		}
		public static void WriteTo(this ICollection items, System.IO.TextWriter writer, bool ownsStream, JsonWriterOptions options)
		{
			JsonWriter.Write(writer, ownsStream, iw => iw.WriteArray(items.Write), options);
		}
	}
}
