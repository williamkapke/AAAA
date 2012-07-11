using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Linq;
using AAAA.PropertyExpressions;

namespace AAAA.JsonSerialization
{
	public interface IJsonObject : ICollection, IEnumerable<KeyValuePair<string, object>>, IJsonItemWriter
	{
		object this[string key] { get; set; }
		ICollection<string> GetKeys();
		bool TryGetValue(string key, out object value);
		bool ContainsKey(string key);
		new IEnumerator<KeyValuePair<string, object>> GetEnumerator();
		string ToJson(JsonWriterOptions options);
		void WriteTo(TextWriter writer, JsonWriterOptions options);
	}
	public static class IJsonObjectExtensions
	{
		public static T Get<T>(this IJsonObject item, string key) where T : IConvertible
		{
			object value = item[key];
			if (value == null) return default(T);
			return (T)Convert.ChangeType(value, typeof(T));
		}
		public static JsonArray<T> Array<T>(this IJsonObject items, string nodename)
		{
			return items[nodename] as JsonArray<T>;
		}
		public static JsonArray<IJsonObject> Array(this IJsonObject items, string nodename)
		{
			var array1 = items[nodename] as JsonArray<object>;
			var array2 = new JsonArray<IJsonObject>(array1.Count);
			array2.AddRange(array1.Cast<IJsonObject>());
			return array2;
		}
		public static IJsonObject Object(this IJsonObject items, string nodename)
		{
			return items[nodename] as IJsonObject;
		}
		public static IEnumerable<T> Enumerable<T>(this IJsonObject items, string nodename)
		{
			return items[nodename] as IEnumerable<T>;
		}
		public static string ToJson(this IJsonObject items, Propex targets)
		{
			return items.ToJson(false, targets: targets);
		}
		public static string ToJson(this IJsonObject items, bool formatted = false, char formatChar = '\t', int maxDepth = JsonWriter.MAX_DEPTH, bool ignoreDirectionRestrictions = false, Propex targets = null)
		{
			JsonWriterOptions options;
			options = new JsonWriterOptions(maxDepth, formatChar, formatted, ignoreDirectionRestrictions, targets);
			return items.ToJson(options);
		}
		/// <summary>
		///		Serialized the model ignoring any [DoNotSerialize] restrictions.
		/// </summary>
		public static void WriteTo(this IJsonObject items, TextWriter writer, Propex targets = null)
		{
			items.WriteTo(writer, new JsonWriterOptions(targets: targets));
		}
	}
}
