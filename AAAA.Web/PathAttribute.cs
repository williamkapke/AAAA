using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AAAA.Web
{
	[global::System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
	public abstract class PathAttribute : Attribute
	{
		public PathAttribute(string uriTemplate)
		{
			UriTemplate = new UriTemplate(uriTemplate.ToLowerInvariant(), false);
		}
		public bool UseGet { get; set; }
		public abstract bool RequiresAuthentication { get; }
		public UriTemplate UriTemplate { get; private set; }
	}
}
