using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AAAA.PropertyExpressions
{
	public class Property
	{
		public string Name { get; private set; }
		public bool IsOptional { get; private set; }
		public Propex SubProperties { get; private set; }

		public Property(string name) : this(name, false, null) { }
		public Property(string name, bool isOptional, Propex subProperties)
		{
			Name = name;
			IsOptional = isOptional;
			SubProperties = subProperties;
		}
		public override string ToString()
		{
			var sb = new StringBuilder();
			ToString(sb);
			return sb.ToString();
		}
		public void ToString(StringBuilder sb)
		{
			if (Name != "-1")
				sb.Append(Name);
			if (SubProperties != null)
				SubProperties.ToString(sb);
			if (IsOptional)
				sb.Append('?');
		}
		public static implicit operator Property(string name)
		{
			return new Property(name);
		}
	}
}
