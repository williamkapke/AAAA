using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace AAAA
{
	internal class QueryStringDebugView
	{
		private QueryString qs;

		public QueryStringDebugView(QueryString queryString)
		{
			this.qs = queryString;
		}

		// Properties
		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public KeyValuePair<string, string>[] Items
		{
			get
			{
				return qs.ToArray();
			}
		}
	}
}
