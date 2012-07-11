using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Collections;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace AAAA.PropertyExpressions
{
	[DebuggerTypeProxy(typeof(PropexDebugView))]
	public sealed class Propex : IEnumerable<Property>, ICollection
	{
		//fields
		private HybridDictionary items;
		private static readonly Dictionary<string, Propex> cache = new Dictionary<string, Propex>();

		//properties
		public int Min { get; private set; }
		public int Max { get; private set; }
		public bool IsArray { get; private set; }
		public int? Marker { get; private set; }
		public int Count { get { return items.Count; } }
		public bool IsSynchronized { get { return false; } }
		public object SyncRoot { get { return ((System.Collections.ICollection)items).SyncRoot; } }
		public Property this[string name]
		{
			get
			{
				return (Property)items[name];
			}
		}
		public ReadOnlyCollection<Property> Properties
		{
			get
			{
				
				var list = new List<Property>(items.Count);
				list.AddRange(items.Values.Cast<Property>());
				return list.AsReadOnly();
			}
		}

		//methods
		/// <summary>
		///		Creates a Propex representing an Object.
		/// </summary>
		internal Propex(Property[] properties, int? marker = null) : this(properties, false, 1, 1, marker) { }
		/// <summary>
		///		Creates a Propex representing an Array.
		/// </summary>
		internal Propex(Property[] properties, int min, int max, int? marker = null) : this(properties, true, min, max, marker) { }
		private Propex(Property[] properties, bool isArray, int min, int max, int? marker)
		{
			if (min < 0)
				throw new ArgumentOutOfRangeException("min", min, "Value is less than 0");
			if (max < min)
				throw new ArgumentException("Value is less than min", "max");
			if (properties == null)
				this.items = new HybridDictionary();
			else
			{
				this.items = new HybridDictionary(properties.Length, true);
				foreach (var target in properties)
					this.items.Add(target.Name, target);
			}
			IsArray = isArray;
			Min = min;
			Max = max;
			Marker = marker;
		}
		public Propex Create(string pattern)
		{
			return PropexReader.Parse(pattern);
		}
		public void CopyTo(Array array, int index)
		{
			((System.Collections.ICollection)items).CopyTo(array, index);
		}
		public IEnumerator<Property> GetEnumerator()
		{
			foreach (DictionaryEntry prop in items)
				yield return (Property)prop.Value;
		}
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
		public static implicit operator Propex(string pattern)
		{
			return PropexReader.Parse(pattern);
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			ToString(sb);
			return sb.ToString();
		}
		internal void ToString(StringBuilder sb)
		{
			sb.Append(IsArray ? '[' : '{');
			bool first = true;
			foreach (DictionaryEntry item in items)
			{
				if (!first)
					sb.Append(',');
				first = false;
				var value = item.Value as Property;
				value.ToString(sb);
			}
			if (Marker.HasValue)
				sb.Append('$').Append(Marker.Value);
			sb.Append(IsArray ? ']' : '}');
			if (IsArray && !(Min == 0 && Max == int.MaxValue))
			{
				if (Min != 0)
					sb.Append(Min);
				if (Max == Min) return;
				sb.Append(':');
				if (Max != int.MaxValue)
					sb.Append(Max);
			}
		}
	}
}
