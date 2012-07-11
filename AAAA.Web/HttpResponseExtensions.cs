using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace AAAA.Web
{
	public static class HttpResponseExtensions
	{
		public static HttpResponse Write(this HttpResponse response, string value1, string value2 = null, string value3 = null, string value4 = null, string value5 = null)
		{
			if (value1 != null) response.Write(value1);
			if (value2 != null) response.Write(value2);
			if (value3 != null) response.Write(value3);
			if (value4 != null) response.Write(value4);
			if (value5 != null) response.Write(value5);
			return response;
		}
	}
}
