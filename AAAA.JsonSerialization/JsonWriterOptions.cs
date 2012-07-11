using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AAAA.PropertyExpressions;

namespace AAAA.JsonSerialization
{
	public class JsonWriterOptions
	{
		public bool Formatted { get; private set; }
		public char FormatChar { get; private set; }
		public int MaxDepth { get; private set; }
		public bool IgnoreDirectionRestrictions { get; private set; }
		public Propex Targets { get; private set; }
		public IList<Action<object>> Augmentors { get; private set; }
		internal char[] chars;

		public JsonWriterOptions() : this(JsonWriter.MAX_DEPTH) { }
		public JsonWriterOptions(int maxDepth = JsonWriter.MAX_DEPTH, char formatChar = '\t', bool formatted = false, bool ignoreDirectionRestrictions = false, Propex targets = null, IList<Action<object>> augmentors = null)
		{
			Formatted = formatted;
			FormatChar = formatChar;
			MaxDepth = maxDepth;
			chars = "".PadRight(maxDepth, formatChar).ToCharArray();
			IgnoreDirectionRestrictions = ignoreDirectionRestrictions;
			Targets = targets;
			Augmentors = augmentors;
		}

		public static implicit operator JsonWriterOptions(bool formatted)
		{
			return new JsonWriterOptions(formatted: formatted);
		}
		public static implicit operator JsonWriterOptions(string targets)
		{
			return new JsonWriterOptions(targets: targets);
		}
		public static implicit operator JsonWriterOptions(Propex targets)
		{
			return new JsonWriterOptions(targets: targets);
		}
	}
}
