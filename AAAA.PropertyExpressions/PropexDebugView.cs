using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Diagnostics;

namespace AAAA.PropertyExpressions
{
	internal class PropexDebugView
	{
		// Fields
		private Propex propex;

		// Methods
		public PropexDebugView(Propex propex)
		{
			this.propex = propex;
		}

		// Properties
		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public Property[] Items
		{
			get
			{
				return propex.Properties.ToArray();
			}
		}
	}
}
