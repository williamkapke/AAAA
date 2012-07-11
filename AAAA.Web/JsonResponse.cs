using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Net;

namespace AAAA.Web
{
	public class JsonResponse
	{
		private HttpResponse response;
		public JsonResponse(HttpResponse response)
		{
			this.response = response;
		}
	}
}
