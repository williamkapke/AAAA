using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AAAA.Web
{
	[global::System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
	public class PrivateAttribute : PathAttribute
	{
		public PrivateAttribute(string uriTemplate) : base(uriTemplate) { }

		/// <summary>
		///		Causes methods to use GET instead of POST. This value is ignored on methods named POST(), PUT(), or DELETE().
		/// </summary>
		public override bool RequiresAuthentication { get { return true; } }
	}
}
