using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Reflection;
using AAAA.Web;
using AAAA.JsonSerialization;

namespace AAAA.Web
{
	public static class WebServices
	{
		public static UriTemplateTable UriTemplateTable = new UriTemplateTable(new Uri("http://localhost"));
		public static void Discover(params Type[] serviceTypes)
		{
			//discover and populate the UriTemplateTable
			foreach (Type service in serviceTypes)
			{
				if (typeof(WebServiceBase) != service.BaseType)
					continue;

				MethodInfo[] methods = service.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
				foreach (MethodInfo method in methods)
				{
					var attr = Attribute.GetCustomAttribute(method, typeof(JsonBinderAttribute), false) as JsonBinderAttribute;
					Type modelType = (attr != null) ? attr.Type : null;

					var attrs = Attribute.GetCustomAttributes(method, typeof(PathAttribute), false);
					foreach (var o in attrs)
					{
						PathAttribute attribute = o as PathAttribute;
						if (attribute == null) continue;

						string verb = method.Name;
						if (verb != "GET" && verb != "PUT" && verb != "POST" && verb != "DELETE")
							verb = attribute.UseGet ? "GET" : "POST";

						WebMethodInfo info = new WebMethodInfo(verb, method, attribute.RequiresAuthentication, modelType, attr == null ? null : attr.Targets);
						UriTemplateTable.KeyValuePairs.Add(new KeyValuePair<UriTemplate, object>(attribute.UriTemplate, info));
					}
				}
			}
		}
	}
}
