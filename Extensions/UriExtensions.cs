using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AAAA
{
	public static class UriExtensions
	{
		public static string Base(this Uri uri)
		{
			return uri.Scheme + "://" + uri.DnsSafeHost;
		}
	}
}
