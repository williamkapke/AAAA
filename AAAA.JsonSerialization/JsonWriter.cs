using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using System.Globalization;
using AAAA.PropertyExpressions;

namespace AAAA.JsonSerialization
{
	public class JsonWriter
	{
		public const int MAX_DEPTH = 8;
		TextWriter writer;
		BitArray notfirst;
		private static readonly JsonWriterOptions defaultOptions = new JsonWriterOptions();
		private JsonWriterOptions options;
		private JsonWriter.Object objWriter;
		private JsonWriter.Array arrWriter;
		private JsonWriter.Item itemWriter;
		private Property current;

		private int depth = 0;

		private JsonWriter(TextWriter writer) : this(writer, null) { }
		private JsonWriter(TextWriter writer, JsonWriterOptions options)
		{
			objWriter = new JsonWriter.Object(this);
			arrWriter = new JsonWriter.Array(this);
			itemWriter = new JsonWriter.Item(this);
			this.writer = writer;
			this.options = options ?? defaultOptions;
			this.notfirst = new BitArray(this.options.MaxDepth + 1);
			this.current = new Property(null, false, this.options.Targets);
		}

		public static void Write(TextWriter writer, bool ownsStream, IJsonItemWriter writableItem, JsonWriterOptions options = null)
		{
			Write(writer, ownsStream, writableItem.Write, options);
		}
		public static void Write(TextWriter writer, bool ownsStream, Action<JsonWriter.Item> resolver, JsonWriterOptions options = null)
		{
			JsonWriter w = new JsonWriter(writer, options);
			if (options == null) options = defaultOptions;
			resolver(w.itemWriter);
			if (ownsStream)
				writer.Close();
		}

		private void WriteQuotedString(string value)
		{
			writer.Write('"');
			for (int i = 0, length = value.Length; i < length; i++)
			{

				char c = value[i];
				if (c == '/' || c == '"' || c == '\\')
				{
					writer.Write('\\');
					writer.Write(c);
				}
				else if (c < ' ' || (c >= 0xd800 && (c <= 0xdfff || c >= 0xfffe)))
				{
					writer.Write('\\');
					writer.Write('u');
					writer.Write(((int)c).ToString("x4", CultureInfo.InvariantCulture));
				}
				else writer.Write(c);
			}
			writer.Write('"');
		}
		private void WriteName(string name)
		{
			WriteQuotedString(name);
			writer.Write(':');
		}
		private void WriteValue(Property property, object value)
		{
			if (value == null)
			{
				writer.Write("null");
				return;
			}
			else if (value is Enum)
				value = value.ToString();
			else if (value.TryAs<IJsonItemWriter>(writer =>
				{
					var prev = current;
					current = property;
					writer.Write(this.itemWriter);
					current = prev;
				}
				))
				return;
			//else if (value.TryAs<JsonItemWriter>(writer => writer(value, this.itemWriter, targets)))
			//    return;

			IConvertible convertable = value as IConvertible;
			if (convertable != null)
			{
				switch (convertable.GetTypeCode())
				{
					case TypeCode.Boolean:
						writer.Write((bool)value ? "true" : "false");
						return;
					case TypeCode.Char:
					case TypeCode.String:
						WriteQuotedString(value.ToString());
						return;
					case TypeCode.Decimal:
					case TypeCode.Double:
					case TypeCode.Int16:
					case TypeCode.Int32:
					case TypeCode.Int64:
					case TypeCode.Single:
						writer.Write(value);
						return;

					case TypeCode.DateTime:
						WriteQuotedString(((DateTime)value).ToUniversalTime().ToString("o"));
						return;

					//case TypeCode.Byte:
					//case TypeCode.DBNull:
					//case TypeCode.Object:
					//case TypeCode.SByte:
					//case TypeCode.UInt16:
					//case TypeCode.UInt32:
					//case TypeCode.UInt64:
					default:
						break;
				}
			}
			if (!value.TryAs<IDictionary>(d => WriteObject(property, d.Write)))
			{
				if (!value.TryAs<ICollection>(c => WriteArray(property, c.Write)))
				{
					WriteValue(property, value.GetType().ToString());
				}
			}
		}
		private void WriteCommaAndFormatting()
		{
			if (notfirst[depth])
				writer.Write(',');
			notfirst[depth] = true;
			WriteFormatting();
		}
		private void WriteFormatting()
		{
			if (!options.Formatted) return;
			writer.WriteLine();
			writer.Write(options.chars, 0, depth);
		}
		private void WriteObject(Property property, Action<JsonWriter.Object> resolver)
		{
			writer.Write('{');
			if (++depth <= options.MaxDepth)
			{
				var prev = current;
				current = property;
				notfirst[depth] = false;
				if (resolver != null)
					resolver(objWriter);
				if (property.SubProperties != null)
				{
					int? marker = property.SubProperties.Marker;
					if (options.Augmentors != null && marker.HasValue && marker.Value < options.Augmentors.Count)
						options.Augmentors[marker.Value](objWriter);
				}
				current = prev;
			}
			depth--;
			WriteFormatting();
			writer.Write('}');
		}
		private void WriteArray(Property property, Action<JsonWriter.Array> resolver)
		{
			writer.Write('[');
			if (++depth <= options.MaxDepth)
			{
				var prev = current;
				current = property;
				notfirst[depth] = false;
				if (resolver != null)
					resolver(arrWriter);

				if (property.SubProperties != null)
				{
					int? marker = property.SubProperties.Marker;
					if (options.Augmentors != null && marker.HasValue && marker.Value < options.Augmentors.Count)
						options.Augmentors[marker.Value](arrWriter);
				}
				current = prev;
			}
			depth--;
			WriteFormatting();
			writer.Write(']');
		}

		public class Object
		{
			private JsonWriter writer;
			internal Object(JsonWriter writer) { this.writer = writer; }

			public Property ParentProperty { get { return writer.current; } }
			public bool IgnoreDirectionRestrictions { get { return writer.options.IgnoreDirectionRestrictions; } }
			public void WriteValue(string name, object value)
			{
				Property property = new Property(name);
				writer.WriteCommaAndFormatting();
				writer.WriteName(property.Name);
				writer.WriteValue(property, value);
			}
			public void WriteValue(Property property, object value)
			{
				if (property.IsOptional && value == null) return;
				writer.WriteCommaAndFormatting();
				writer.WriteName(property.Name);
				writer.WriteValue(property, value);
			}
			public void WriteValue(Property property, Action<JsonWriter.Item> itemWriter)
			{
				var previous = writer.current;
				writer.current = property;
				writer.WriteCommaAndFormatting();
				writer.WriteName(property.Name);
				itemWriter(writer.itemWriter);
				writer.current = previous;
			}
			public void WriteObject(Property property, Action<JsonWriter.Object> objWriter)
			{
				writer.WriteCommaAndFormatting();
				writer.WriteName(property.Name);
				writer.WriteObject(property, objWriter);
			}
			public void WriteArray(Property property, Action<JsonWriter.Array> arrWriter)
			{
				writer.WriteCommaAndFormatting();
				writer.WriteName(property.Name);
				writer.WriteArray(property, arrWriter);
			}
			//public void WriteArray(Property property, params object[] items)
			//{
			//    WriteArray(property, (w, t) =>
			//    {
			//        for (int i = 0; i < items.Length; i++)
			//            w.WriteValue(items[i]);
			//    });
			//}
		}
		public class Array
		{
			private JsonWriter writer;
			internal Array(JsonWriter writer) { this.writer = writer; }

			public Property ParentProperty { get { return writer.current; } }
			public bool IgnoreDirectionRestrictions { get { return writer.options.IgnoreDirectionRestrictions; } }
			public void WriteValue(Property property, object value)
			{
				writer.WriteCommaAndFormatting();
				if (value is Undefined) return;
				writer.WriteValue(property, value);
			}
			public void WriteObject(Property property, Action<JsonWriter.Object> objWriter)
			{
				writer.WriteCommaAndFormatting();
				writer.WriteObject(property, objWriter);
			}
			public void WriteArray(Property property, Action<JsonWriter.Array> arrWriter)
			{
				writer.WriteCommaAndFormatting();
				writer.WriteArray(property, arrWriter);
			}
			//public void WriteArray(params object[] items)
			//{
			//    WriteArray((w, t) =>
			//    {
			//        for (int i = 0; i < items.Length; i++)
			//            w.WriteValue(items[i]);
			//    });
			//}
		}
		public class Item
		{
			private JsonWriter writer;
			internal Item(JsonWriter writer) { this.writer = writer; }

			public Property CurrentProperty { get { return writer.current; } }
			public bool IgnoreDirectionRestrictions { get { return writer.options.IgnoreDirectionRestrictions; } }
			public void WriteValue(object value)
			{
				if (CurrentProperty.IsOptional && value == null) return;
				writer.WriteValue(CurrentProperty, value);
			}
			public void WriteObject(Action<JsonWriter.Object> objWriter)
			{
				writer.WriteObject(CurrentProperty, objWriter);
			}
			public void WriteArray(Action<JsonWriter.Array> arrWriter)
			{
				writer.WriteArray(CurrentProperty, arrWriter);
			}
			//public void WriteArray(params object[] items)
			//{
			//    WriteArray((w, t) =>
			//    {
			//        for (int i = 0; i < items.Length; i++)
			//            w.WriteValue(items[i]);
			//    });
			//}
		}
	}
}
