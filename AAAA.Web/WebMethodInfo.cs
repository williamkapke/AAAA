using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections.Specialized;
using System.Web;
using AAAA.PropertyExpressions;

namespace AAAA.Web
{
	public sealed class WebMethodInfo
	{
		public WebMethodInfo(string verb, MethodInfo method, bool requiresAuth, Type modelType, Propex targets)
		{
			this.Verb = verb;
			this.method = method;
			this.RequiresAuthentication = requiresAuth;
			this.Targets = targets;
			this.Parameters = method.GetParameters();
			this.ModelType = modelType;
		}
		public readonly string Verb;
		private readonly MethodInfo method;
		public readonly bool RequiresAuthentication;
		public readonly Propex Targets;
		public readonly ParameterInfo[] Parameters;

		//this is the type of JSON object that is return to the client
		public readonly Type ModelType;

		public Type DeclaringType { get { return method.DeclaringType; } }
		public string Name { get { return method.Name; } }

		public object Invoke(object targetInstance, params object[] parameters)
		{
			return method.Invoke(targetInstance, parameters);
		}
	}
}
