using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections;

namespace AAAA.JsonSerialization
{
	internal class JsonObjectDebugView
	{
		// Fields
		private ICollection collection;

		// Methods
		public JsonObjectDebugView(ICollection collection)
		{
			this.collection = collection;
		}

		// Properties
		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public KeyValuePair<string, object>[] Items
		{
			get
			{
				var kvps = new KeyValuePair<string, object>[this.collection.Count];
				int count = 0;
				foreach (DictionaryEntry item in collection)
				{
					kvps[count++] = new KeyValuePair<string, object>(item.Key.ToString(), item.Value);
				}
				return kvps;
			}
		}
	}
}


