using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Runtime.Serialization.Json;

namespace AAAA.JsonSerialization
{
	public class JsonReader
	{
		private JsonReader() { }
		internal static void Read(XmlReader reader, Action<string, object> add)
		{
			string rootName = reader.Name;

			while (reader.Read())
			{
				if (reader.IsStartElement())
				{
					object nodeValue = null;
					string nodeName = reader.Name;
					reader.MoveToAttribute("type");
					string nodeType = reader.Value;
					reader.MoveToContent();

					switch (nodeType)
					{
						case "object":
							var obj = new JsonObject();
							Read(reader, obj.AddOrUpdate);
							nodeValue = obj;
							break;
						case "array":
							var list = new JsonArray<object>();
							Read(reader, (n, v) =>
							{
								JsonObject.EnsureValidType(v);
								list.Add(v);
							});
							nodeValue = list;//.ToArray();
							break;
						case "null":
							nodeValue = null;
							break;
						case "number":
							string value = reader.ReadString().Trim();
							value.TryConvertTo(TypeCode.Double, out nodeValue);
							break;
						default:
							nodeValue = reader.ReadString().Trim();
							break;
					}
					add(nodeName, nodeValue);
				}
				else if (reader.Name == rootName)
					break;
			}
		}

		public static JsonArray<object> ReadArray(string json)
		{
			var items = new JsonArray<object>();
			using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
			{
				ReadFromStream(ms, (k, v) => items.Add(v));
				ms.Close();
			}
			return items;
		}
		public static JsonArray<T> ReadArray<T>(string json)
		{
			var items = new JsonArray<T>();
			using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
			{
				ReadFromStream(ms, (k, v) => items.Add((T)v));
				ms.Close();
			}
			return items;
		}
		public static JsonArray<T> ReadArray<T>(Stream stream)
		{
			var items = new JsonArray<T>();
			ReadFromStream(stream, (k, v) => items.Add((T)v));
			return items;
		}
		public static JsonObject ReadObject(string json)
		{
			JsonObject items = new JsonObject();
			using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
			{
				ReadFromStream(ms, items.AddOrUpdate);
				ms.Close();
			}
			return items;
		}
		public static JsonObject ReadObject(Stream stream)
		{
			var obj = new JsonObject();
			ReadFromStream(stream, obj.AddOrUpdate);
			return obj;
		}
		private static void ReadFromStream(Stream stream, Action<string, object> add)
		{
			XmlDictionaryReader reader = JsonReaderWriterFactory.CreateJsonReader(stream, Encoding.UTF8, XmlDictionaryReaderQuotas.Max, null);
			if (reader.Read() && reader.Name == "root")
				JsonReader.Read(reader, add);
		}
	}
}
