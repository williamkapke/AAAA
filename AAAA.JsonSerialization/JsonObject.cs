using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.IO;
using System.Xml;
using System.Collections;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Collections.Specialized;
using System.Diagnostics;
using AAAA.PropertyExpressions;

namespace AAAA.JsonSerialization
{
	[DebuggerTypeProxy(typeof(JsonObjectDebugView)), DebuggerDisplay("{ToJson()}")]
	public class JsonObject : IJsonObject, IDictionary<string, object>, IDictionary
	{
		HybridDictionary source;

		public JsonObject()
		{
			source = new HybridDictionary();
		}
		public JsonObject(int capacity)
		{
			source = new HybridDictionary(capacity);
		}
		public static implicit operator JsonObject(string value)
		{
			return JsonReader.ReadObject(value);
		}

		public void AddOrUpdate(string key, object value)
		{
			EnsureValidType(value);
			source[key] = value;
		}
		internal static bool EnsureValidType(object value)
		{
			if (value == null || value is IConvertible || value is IJsonItemWriter)
				return true;
			if (value is IEnumerable)
			{
				//TODO: check what type of items it has
				return true;
			}
			throw new ArgumentException("JsonObject cannot contain items of type: " + value.GetType().ToString());
		}
		public string ToJson(bool formatted = false, char formatChar = '\t', int maxDepth = JsonWriter.MAX_DEPTH, bool ignoreDirectionRestrictions = false, Propex targets = null)
		{
			return ((IDictionary)this).ToJson(formatted, formatChar, maxDepth, ignoreDirectionRestrictions, targets);
		}
		public string ToJson(JsonWriterOptions options)
		{
			return ((IDictionary)this).ToJson(options);
		}
		public void Write(JsonWriter.Item iw)
		{
			iw.WriteObject(((IDictionary)this).Write);
		}
		public void WriteTo(TextWriter writer, JsonWriterOptions options)
		{
			JsonWriter.Write(writer, false, iw => iw.WriteObject(this.Write), options);
		}
		public ICollection<string> GetKeys()
		{
			return source.Keys.Cast<string>().ToArray();
		}

		#region HybridDictionary wrappers
		ICollection IDictionary.Keys
		{
			get { return source.Keys; }
		}
		ICollection<string> IDictionary<string, object>.Keys
		{
			get { return GetKeys(); }
		}
		public ICollection<object> Values
		{
			get { return source.Values.Cast<object>().ToArray(); }
		}
		ICollection IDictionary.Values
		{
			get { return source.Values; }
		}
		public object this[string key]
		{
			get
			{
				return source[key];
			}
			set
			{
				source[key] = value;
			}
		}
		object IDictionary.this[object key]
		{
			get
			{
				return this[(string)key];
			}
			set
			{
				this[(string)key] = value;
			}
		}
		bool IDictionary.IsFixedSize
		{
			get { return source.IsFixedSize; }
		}
		public bool IsReadOnly
		{
			get { return source.IsReadOnly; }
		}
		bool ICollection.IsSynchronized
		{
			get { return source.IsSynchronized; }
		}
		object ICollection.SyncRoot
		{
			get { return source.SyncRoot; }
		}
		public int Count
		{
			get { return source.Count; }
		}

		public void Add(string key, object value)
		{
			AddOrUpdate(key, value);
		}
		void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item)
		{
			Add(item.Key, item.Value);
		}
		void IDictionary.Add(object key, object value)
		{
			Add((string)key, value);
		}

		public bool ContainsKey(string key)
		{
			return source.Contains(key);
		}
		bool IDictionary.Contains(object key)
		{
			return ContainsKey((string)key);
		}
		bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item)
		{
			if (!ContainsKey(item.Key)) return false;
			var value = source[item.Key];
			return (value == item.Value);
		}

		void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
		{
			throw new NotImplementedException();
		}
		public void CopyTo(Array array, int index)
		{
			source.CopyTo(array, index);
		}

		public bool TryGetValue(string key, out object value)
		{
			value = source[key];
			return ContainsKey(key);
		}
		public void Clear()
		{
			source.Clear();
		}

		public bool Remove(string key)
		{
			bool exists = ContainsKey(key);
			source.Remove(key);
			return exists;
		}
		void IDictionary.Remove(object key)
		{
			source.Remove(key);
		}
		bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item)
		{
			throw new NotImplementedException();
		}

		public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
		{
			foreach (DictionaryEntry item in source)
				yield return new KeyValuePair<string, object>((string)item.Key, item.Value);
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return source.GetEnumerator();
		}
		IDictionaryEnumerator IDictionary.GetEnumerator()
		{
			return source.GetEnumerator();
		}
		#endregion
	}
}
