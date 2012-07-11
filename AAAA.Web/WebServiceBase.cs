using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Routing;
using AAAA.JsonSerialization;
using AAAA.PropertyExpressions;
using System.Net;
using System.Reflection;
using System.Collections.Specialized;
using System.IO;

namespace AAAA.Web
{
	//this exists just so Extansion methods can attach to all webservices
	public interface IWebService : IHttpHandler, IRouteHandler { }

	public abstract class WebServiceBase : IWebService
	{
		protected Propex Targets { get; private set; }
		protected bool Formatted { get { return requestContext.HttpContext.Request.QueryString["formatted"] != null; } }
		protected RouteData RouteData { get; private set; }
		protected UriTemplateMatch UriTemplateMatch { get; private set; }
		public bool IsReusable { get { return false; } }

		private RequestContext requestContext;

		protected JsonRequest Request
		{
			get
			{
				return request ?? (request = new JsonRequest(requestContext.HttpContext.Request));
			}
		}
		private JsonRequest request;

		protected ResponseAction OK(IJsonObject jsonObject, Propex targets = null)
		{
			return Respond(HttpStatusCode.OK, jsonObject, targets ?? Targets);
		}
		protected ResponseAction OK(IJsonItemWriter itemWriter, Propex targets = null)
		{
			return Respond(HttpStatusCode.OK, itemWriter, targets);
		}
		protected ResponseAction OK(System.Collections.ICollection models, Propex targets)
		{
			JsonWriterOptions options = new JsonWriterOptions(targets: targets, formatted: Formatted);
			return OK(models, options);
		}
		protected ResponseAction OK(System.Collections.ICollection models, JsonWriterOptions options)
		{
			return r =>
			{
				r.StatusCode = (int)HttpStatusCode.OK;
				models.WriteTo(r.Output, false, options);
			};
		}
		protected ResponseAction OK(string xmessage)
		{
			return Respond(HttpStatusCode.OK, null, xmessage);
		}
		protected ResponseAction OK()
		{
			return Respond(HttpStatusCode.OK);
		}
		/// <summary>
		///		Give a 400 Bad Request response and writes the error X-Message header
		/// </summary>
		protected ResponseAction BadRequest(string responseText, string xcode, string xmessage)
		{
			return Respond(HttpStatusCode.BadRequest, responseText, xcode, xmessage);
		}
		/// <summary>
		///		Provides an efficient way to return a single object error: {"name":"error"}
		/// </summary>
		protected ResponseAction BadRequest(string name, string error)
		{
			return Respond(HttpStatusCode.BadRequest, "{\"" + name + "\":\"" + HttpUtility.HtmlAttributeEncode(error) + "\"}");
		}
		protected ResponseAction BadRequest(JsonObject errors)
		{
			return Respond(HttpStatusCode.BadRequest, errors, null);
		}
		protected ResponseAction Unauthorized(string xmessage)
		{
			return Respond(HttpStatusCode.Unauthorized, null, xmessage);
		}

		protected ResponseAction Created(IJsonObject jsonObject, Propex targets)
		{
			return Respond(HttpStatusCode.Created, jsonObject, targets);
		}

		protected ResponseAction Respond(HttpStatusCode status)
		{
			return r => r.StatusCode = (int)status;
		}
		protected ResponseAction Respond(HttpStatusCode status, string responseText)
		{
			return r =>
			{
				r.StatusCode = (int)status;
				r.Write(responseText);
			};
		}
		protected ResponseAction Respond(HttpStatusCode status, string xcode, string xmessage)
		{
			return Respond(status, null, xcode, xmessage);
		}
		protected ResponseAction Respond(HttpStatusCode status, string responseText, string xcode, string xmessage)
		{
			return r =>
			{
				r.StatusCode = (int)status;

				if (!String.IsNullOrEmpty(xcode))
					r.Headers["X-ResultCode"] = HttpUtility.HtmlAttributeEncode(xcode);

				if (!String.IsNullOrEmpty(xmessage))
					r.Headers["X-Message"] = HttpUtility.HtmlAttributeEncode(xmessage);

				if (!String.IsNullOrEmpty(responseText))
					r.Write(responseText);
			};
		}
		protected ResponseAction Respond(HttpStatusCode status, IJsonObject jsonObject, Propex targets)
		{
			var options = new JsonWriterOptions(targets: targets, formatted: Formatted);
			return Respond(status, (IJsonItemWriter)jsonObject, options);
		}
		protected ResponseAction Respond(HttpStatusCode status, IJsonItemWriter itemWriter, JsonWriterOptions options)
		{
			return r =>
			{
				r.StatusCode = (int)status;
				JsonWriter.Write(r.Output, false, itemWriter.Write, options);
			};
		}


		void IHttpHandler.ProcessRequest(HttpContext context)
		{
			var callback = context.Request.QueryString["callback"];
			bool useCallback = !callback.IsNullOrEmpty();
			if (useCallback && !callback.IsMatch("^[a-zA-Z0-9]+$"))
			{
				context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
				context.Response.Headers["X-Message"] = "Callback must match ^[a-zA-Z0-9]+$";
				return;
			}

			context.Response.ContentType = useCallback ? "text/javascript" : "application/json";
			UriTemplateMatch = (UriTemplateMatch)RouteData.Values["UriTemplateMatch"];
			WebMethodInfo methodinfo = (WebMethodInfo)UriTemplateMatch.Data;
			Targets = methodinfo.Targets;
			ParameterInfo[] parameters = methodinfo.Parameters;
			object[] paramsArray = new object[parameters.Length];
			object model = null;
			string urlerror = null;
			bool hasUrlError = false;
			bool hasError = false;

			for (int i = 0; i < parameters.Length; i++)
			{
				ParameterInfo parameter = parameters[i];

				//if the parameter is TModel, serialize the input
				var paramType = parameter.ParameterType;
				if (paramType.Equals(methodinfo.ModelType))
				{
					if (methodinfo.Verb != "PUT" && methodinfo.Verb != "POST")
						throw new InvalidOperationException("WebService methods must be PUT or POST to use TModel as a parameter.");

					//serialize the JsonObject
					if (model != null)
						throw new InvalidOperationException("WebService method has duplicate TModel parameters.");

					try
					{
						object parseresult;
						var parser = paramType.GetParserDelegate("Parse");
						if (parser == null)
							throw new InvalidOperationException("Type '" + paramType + "' does not have a static 'Parse' method matching the ObjectParser delegate.");
						hasError = !parser(Request.JsonObject, methodinfo.Targets, out parseresult);

						if (hasError)
						{
							((IJsonObject)parseresult).WriteTo(context.Response.Output);
						}
						else
							model = paramsArray[i] = parseresult;
					}
					catch (System.Xml.XmlException)
					{
						context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
						if (urlerror != null)
							context.Response.Headers["X-Message"] = "Invalid JSON";
						return;
					}
				}
				else //assign method parameters from url variables
				{
					//we only report 1 url error, so skip the others
					if (hasUrlError) continue;

					object o;
					string error = BindParameterValue(parameter, out o);
					if (!String.IsNullOrEmpty(error))
					{
						hasUrlError = hasError = true;
						urlerror = error;
					}
					else paramsArray[i] = o;
				}
			}

			if (hasError)
			{
				context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
				if (urlerror != null)
					context.Response.Headers["X-Message"] = urlerror;
				return;
			}

			ResponseAction result = methodinfo.Invoke(this, paramsArray) as ResponseAction;
			context.Response.Charset = "utf-8";
			if (result != null)
			{
				if (useCallback)
				{
					context.Response.Write(callback);
					context.Response.Write('(');
				}
				result(requestContext.HttpContext.Response);
				if (useCallback)
					context.Response.Write(')');
			}
		}
		IHttpHandler IRouteHandler.GetHttpHandler(RequestContext requestContext)
		{
			this.requestContext = requestContext;
			this.RouteData = requestContext.RouteData;
			return this;
		}

		protected virtual string BindParameterValue(System.Reflection.ParameterInfo parameter, out object value)
		{
			if (!typeof(IConvertible).IsAssignableFrom(parameter.ParameterType))
				throw new InvalidOperationException("WebService method parameters must impliment IConvertable.");

			string variable = UriTemplateMatch.BoundVariables[parameter.Name];

			value = null;

			if (variable == null || variable.TryParse(parameter.ParameterType, out value))
				return null;

			return "Invalid url parameter: " + parameter.Name + "=" + (variable ?? "null");
		}
	}
}
