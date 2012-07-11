using System.IO;
using System;

namespace AAAA.JsonSerialization
{
	public delegate void JsonItemWriter(object value, JsonWriter.Item iw);

	public interface IJsonItemWriter
	{
		void Write(JsonWriter.Item iw);
	}
}
