using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;

namespace AAAA.JsonSerialization
{
	public class JsonArray<T> : List<T>
	{
		public JsonArray() { }
		public JsonArray(int capacity) : base(capacity) { }
		//public JsonArray(string json)
		//{
		//    if (String.IsNullOrEmpty(json))
		//        return;
		//    List<object> items = JsonReader.ReadArray(json);
		//    using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
		//    {
		//        XmlDictionaryReader reader = JsonReaderWriterFactory.CreateJsonReader(ms, Encoding.UTF8, XmlDictionaryReaderQuotas.Max, null);
		//        if (reader.Read() && reader.Name == "root")
		//            JsonReader.Read(reader, (k, v) => Add(v));
		//        ms.Close();
		//    }
		//}
		//public static bool Parse(object value, JsonTargetCollection targets, out object result)
		//{
		//    var sub = new JsonPropertyParser.Custom(SubParse);
		//    var arrayParser = new JsonPropertyParser.List<object>(sub);
		//    return arrayParser.Parse(value, targets, out result);
		//}
		//private static bool SubParse(object value, JsonTargetCollection targets, out object result)
		//{
		//    result = null;
		//    if (!value.TryAs<IConvertible>(item => { }))
		//    {
		//    }
		//    return false;
		//}
	}
	public static class JsonArrayExtensions
	{
		public static T Pop<T>(this JsonArray<T> items)
		{
			int index = items.Count - 1;
			if (index < 0) return default(T);
			var item = items[index];
			items.RemoveAt(index);
			return item;
		}
		public static IJsonObject Object<T>(this JsonArray<T> item, int index)
		{
			return item[index] as IJsonObject;
		}
		public static IJsonObject Object<T>(this IJsonObject item, string name)
		{
			return item[name] as IJsonObject;
		}
		public static JsonArray<T> Array<T>(this JsonArray<T> item, int index)
		{
			return item[index] as JsonArray<T>;
		}
		public static T Get<T>(this JsonArray<T> item, int index) where T : IConvertible
		{
			object value = item[index];
			if (value == null) return default(T);
			return (T)Convert.ChangeType(value, typeof(T));
		}
	}
}
