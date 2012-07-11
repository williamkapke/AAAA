using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using AAAA.JsonSerialization;

namespace AAAA.Web
{
	public class JsonRequest
	{
		private HttpRequestBase request;
		public JsonRequest(HttpRequestBase request)
		{
			this.request = request;
		}

		public JsonObject JsonObject
		{
			get
			{
				if (jsonObject == null)
				{
					string json = null;
					using (var sr = new System.IO.StreamReader(request.InputStream))
					{
						json = sr.ReadToEnd();
					}
					jsonObject = JsonReader.ReadObject(json);
				}
				return jsonObject;
			}
		}
		private JsonObject jsonObject;

		public string UserHostAddress
		{
			get { return request.UserHostAddress; }
		}
	}
}
