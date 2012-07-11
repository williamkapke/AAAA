using System;
using AAAA;
using AAAA.Web;
using System.Web;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Routing;
using System.Net;
using System.Security.Principal;

namespace AAAA.Web
{
	public abstract class ControllerBase<T> : IHttpHandler, IRouteHandler
	{
		private RequestContext requestContext;
		public bool IsReusable { get { return false; } }
		protected UriTemplateMatch UriTemplateMatch { get; private set; }
		protected RouteData RouteData { get; private set; }
		protected IPrincipal User { get { return requestContext.HttpContext.User; } }

		protected DateTime? IfModifiedSince
		{
			get
			{
				string header = requestContext.HttpContext.Request.Headers["If-Modified-Since"];
				if (header == null) return null;

				DateTime date;
				return DateTime.TryParse(header, out date) ? date : (DateTime?)null;
			}
		}

		void IHttpHandler.ProcessRequest(HttpContext context)
		{
			UriTemplateMatch = (UriTemplateMatch)RouteData.Values["UriTemplateMatch"];
			WebMethodInfo method = (WebMethodInfo)UriTemplateMatch.Data;

			ParameterInfo[] parameters = method.Parameters;
			object[] paramsArray = new object[parameters.Length];

			for (int i = 0; i < parameters.Length; i++)
			{
				ParameterInfo parameter = parameters[i];

				paramsArray[i] = BindParameterValue(parameter);
			}

			ActionResult result = method.Invoke(this, paramsArray) as ActionResult;
			requestContext.HttpContext.Response.Charset = "utf-8";
			if (result != null)
				result.ExecuteResult(new ControllerContext() { HttpContext = requestContext.HttpContext, RequestContext = requestContext, RouteData = RouteData });
		}
		protected virtual object BindParameterValue(ParameterInfo parameter)
		{
			if (!typeof(IConvertible).IsAssignableFrom(parameter.ParameterType))
				throw new InvalidOperationException("WebService method parameters must impliment IConvertable.");

			object o = null;
			string variable = UriTemplateMatch.BoundVariables[parameter.Name];

			if (variable == null && parameter.ParameterType.IsValueType)
				return Activator.CreateInstance(parameter.ParameterType);

			variable.TryParse(parameter.ParameterType, out o);

			return o;
		}

		protected internal ViewResult View(string viewName)
		{
			return this.View(viewName, null, null);
		}

		protected internal ViewResult View(string viewName, object model)
		{
			return this.View(viewName, null, model);
		}

		protected internal ViewResult View(string viewName, string masterName)
		{
			return this.View(viewName, masterName, null);
		}

		protected internal virtual ViewResult View(string viewName, string masterName, object model)
		{
			ViewResult result = new ViewResult();
			result.ViewName = viewName;
			result.MasterName = masterName;
			result.ViewData = new ViewDataDictionary(model);
			return result;
		}

		protected ActionResult NotModified()
		{
			return new RawActionResult(r => { r.StatusCode = 304; });
		}

		public ActionResult Respond(ResponseAction response)
		{
			return new RawActionResult(response);
		}

		public IHttpHandler GetHttpHandler(RequestContext requestContext)
		{
			this.requestContext = requestContext;
			this.RouteData = requestContext.RouteData;
			return this;
		}
	}
}
