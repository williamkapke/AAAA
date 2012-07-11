using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.IO;
using AAAA.PropertyExpressions;

namespace AAAA.JsonSerialization
{
	public static class IDictionaryExtensions
	{
		public static string ToJson(this IDictionary items, bool formatted = false, char formatChar = '\t', int maxDepth = JsonWriter.MAX_DEPTH, bool ignoreDirectionRestrictions = false, Propex targets = null)
		{
			JsonWriterOptions options = new JsonWriterOptions(maxDepth, formatChar, formatted, ignoreDirectionRestrictions, targets);
			return items.ToJson(options);
		}
		public static string ToJson(this IDictionary items, JsonWriterOptions options)
		{
			var sw = new System.IO.StringWriter();
			JsonWriter.Write(sw, true, iw => iw.WriteObject(items.Write), options);
			return sw.ToString();
		}
		public static void Write(this IDictionary items, JsonWriter.Object ow)
		{
			var targets = ow.ParentProperty.SubProperties;
			if (targets == null)
				foreach (DictionaryEntry item in items)
				{
					if (!(item.Key is IConvertible))
						throw new InvalidOperationException("Cannot serialize Dictionary with non-IConvertable keys.");
					//var value = item.Value;
					//if (!value.TryAs<IDictionary>(d => ow.WriteObject(item.Key.ToString(), d.Write)))
					//    if (value is IConvertible || !value.TryAs<ICollection>(c => ow.WriteArray(item.Key.ToString(), c.Write)))
					ow.WriteValue(new Property((string)item.Key), item.Value);
				}
			else
				foreach (var target in targets)
				{
					bool hasItem = items.Contains(target.Name);
					if (!hasItem && target.IsOptional) continue;
					object value = hasItem ? items[target.Name] : null;
					ow.WriteValue(target, value);
				}
		}
	}
}
