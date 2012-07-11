using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Web;
using System.Diagnostics;

namespace AAAA
{
	[DebuggerTypeProxy(typeof(QueryStringDebugView))]
	public class QueryString : Dictionary<string, string>
	{
		public QueryString() { }
		public QueryString(int capacity) : base(capacity) { }

		public static QueryString Parse(string value)
		{
			int qmarkIndex = value.IndexOf('?');
			if (qmarkIndex == value.Length - 1)
				return new QueryString();
			if (qmarkIndex != -1)
				value = value.Substring(qmarkIndex + 1);

			var pairs = value.Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
			var result = new QueryString(pairs.Length);
			foreach (var pair in pairs)
			{
				var eq = pair.IndexOf('=');
				if (eq == 0) continue; // in cases where name is missing: &=foo
				var name = HttpUtility.UrlDecode(eq == -1 ? pair : pair.Substring(0, eq));
				var val = eq == -1 || eq + 1 == pair.Length ? "" : HttpUtility.UrlDecode(pair.Substring(eq + 1));
				if (result.ContainsKey(name))
					result[name] = val;
				else result.Add(name, val);
			}
			return result;
		}
		public override string ToString()
		{
			return this.ToUrlString();
		}
	}
}
