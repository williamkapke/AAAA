using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace AAAA.Web
{
	public class RawActionResult : ActionResult
	{
		public RawActionResult(ResponseAction response)
		{
			Response = response;
		}
		public ResponseAction Response { get; set; }

		public override void ExecuteResult(ControllerContext context)
		{
			Response(context.HttpContext.Response);
		}
	}
}
