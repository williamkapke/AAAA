using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Routing;

namespace AAAA.Web
{
	public class RawHttpRouteHandler : IRouteHandler, IHttpHandler
	{
		public RawHttpRouteHandler(Action<HttpContext> processRequest)
		{
			this.processRequest = processRequest;
		}
		private Action<HttpContext> processRequest;

		public IHttpHandler GetHttpHandler(RequestContext requestContext) { return this; }
		public bool IsReusable { get { return false; } }
		public void ProcessRequest(HttpContext context) { processRequest(context); }
	}
}
