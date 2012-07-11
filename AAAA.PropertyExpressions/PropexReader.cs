using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AAAA.PropertyExpressions
{
	/*
	PROPERTYGROUP | ARRAYGROUP
	PROPERTYGROUP	::= '{' + PROPERTIES? MARKER? + '}'
	ARRAYGROUP		::= '[' + (PROPERTYGROUP | INDEXITEMS | PROPERTYGROUP(',' + INDEXITEMS))? MARKER? + ']' QUANTITY?
	PROPERTIES		::= PROPERTY(',' + PROPERTY)*
	INDEXITEMS		::= INDEXITEM(',' + INDEXITEM)*
	PROPERTY		::= NAME (ARRAYGROUP | PROPERTYGROUP)? OPTIONAL?
	INDEXITEM		::= NUMBER (ARRAYGROUP | PROPERTYGROUP)? OPTIONAL?
	QUANTITY		::= NUMBER | MIN + ':' | ':' + MAX | MIN + ':' + MAX
	MARKER			::= '$' + NUMBER
	OPTIONAL		::= '?'
	MIN				::= NUMBER
	MAX				::= NUMBER
	NUMBER			::= [0-9]+

	PPP,						min=0 max=		Required
	PPP?						min=0 max=		Optional
	PPP{PPP}?					min=0 max=		Required
	PPP[PPP]?					min=0 max=		Optional
	PPP[PPP]5?					min=5 max=5		Optional
	PPP[PPP]1:5?				min=1 max=5		Optional
	PPP[0{PPP},1,2[PPP]]1:5?	min=1 max=5		Optional

	PPP[0{PPP$2},2[PPP$1]$1]1:5?

	Departments[0{Name,Phone,Hours},1{Name,Phone?,Hours?}]1:5?
	*/
	public class PropexReader
	{
		private static Dictionary<string, Propex> cache = new Dictionary<string, Propex>();
		private string source;
		private char current;
		private int position;
		private int length;
		private int remaining;

		private PropexReader(string source)
		{
			this.source = source;
			position = -1;
			length = source.Length;
			remaining = length;
		}
		public static Propex Parse(string pattern)
		{
			Propex propex;
			if (cache.TryGetValue(pattern, out propex))
				return propex;

			////remove whitespace and try again...
			//pattern = pattern.Replace(new Regex("\\s", RegexOptions.Compiled), "");
			//if (cache.TryGetValue(pattern, out propex))
			//{
			//    cache.Add(pattern, propex);
			//    return propex;
			//}
			propex = new PropexReader(pattern).Parse();
			cache.Add(pattern, propex);
			return propex;
		}
		private Propex Parse()
		{
			Propex propex;
			char c = source[0];
			switch (c)
			{
				case '{':
					propex = ReadPropertyGroup();
					break;
				case '[':
					propex = ReadArrayGroup();
					break;
				default:
					throw new ArgumentException("Pattern must start with '{' or '['", "pattern");
			}
			if (remaining != 0)
				throw new ApplicationException("Unexpected character(s) at the end of the Propex.");

			return propex;
		}
		private Propex ReadPropertyGroup()
		{
			//we start here 1 character before a '{'
			Move(1);
			var props = Peek(1) == '}' ? new Property[0] : ReadProperties();
			Move(1);

			int? marker = null;
			if (current == '$')
			{
				marker = ReadNumber();
				Move(1);
			}
			else marker = null;
			if (current != '}')
				throw new ApplicationException("Unexpected character '" + current + "' in Propex.");
			return new Propex(props.ToArray(), marker);
		}
		private Property[] ReadProperties()
		{
			if (Peek(1) == '$') return new Property[0];
			var props = new List<Property>();
			props.Add(ReadProperty());
			while (Peek(1) == ',')
			{
				Move(1);
				props.Add(ReadProperty());
			}
			return props.ToArray();
		}
		private Property ReadProperty()
		{
			return ReadProperty(ReadPropertyName());
		}
		private Property ReadProperty(string name)
		{
			bool isOptional = false;
			Propex subproperties = null;
			char c = Peek(1);
			switch (c)
			{
				case '{':
					subproperties = ReadPropertyGroup();
					c = Peek(1);
					break;
				case '[':
					subproperties = ReadArrayGroup();
					c = Peek(1);
					break;
			}
			if (c == '?')
			{
				isOptional = true;
				Move(1);
			}
			return new Property(name, isOptional, subproperties);
		}
		private Property ReadIndexItem()
		{
			return ReadProperty(ReadNumber().ToString());
		}
		private int ReadNumber()
		{
			char c;

			int len = 0;
			while (true)
			{
				if (++len > remaining) break;
				c = Peek(len);
				if (!c.IsDigit()) break;
			}
			len--;
			if (len < 1)
				throw new ApplicationException("Invalid Propex pattern.");

			var name = source.Substring(position + 1, len);
			Move(len);
			return int.Parse(name);
		}
		private string ReadPropertyName()
		{
			char c = Peek(1);
			if (!IsValidFirstChar(c))
				throw new ApplicationException("Unexpected character '" + c + "' in Propex.");

			int len = 1;
			while (true)
			{
				c = Peek(++len);
				if (!c.IsAsciiLetter() && !c.IsDigit()) break;
			}
			len--;
			var name = source.Substring(position + 1, len);
			Move(len);
			return name;
		}
		private Propex ReadArrayGroup()
		{
			//we start here 1 character before a '['
			Move(1);
			char c = Peek(1);
			List<Property> indexitems = new List<Property>();
			int? marker;

			if (c == '{')
			{
				indexitems.Add(new Property("-1", false, ReadPropertyGroup()));
				c = Peek(1);
				if (c == ',')
				{
					Move(1);
					c = Peek(1);
				}
			}

			if (c.IsDigit())
				indexitems.AddRange(ReadIndexItems());

			Move(1);
			int min = 0;
			int max = int.MaxValue;
			if (current == '$')
			{
				marker = ReadNumber();
				Move(1);
			}
			else marker = null;

			if (current != ']')
				throw new ApplicationException("Unexpected character '" + current + "' in pattern.");

			if (remaining > 0)
				ReadQuantity(ref min, ref max);
			return new Propex(indexitems.ToArray(), min, max, marker);
		}

		private void ReadQuantity(ref int min, ref int max)
		{
			char c = Peek(1);
			if (c.IsDigit())
			{
				min = max = ReadNumber();
				if (remaining == 0) return;
				c = Peek(1);
			}

			if (c == ':')
			{
				Move(1);
				max = remaining != 0 && Peek(1).IsDigit() ? ReadNumber() : int.MaxValue;
			}
		}
		private Property[] ReadIndexItems()
		{
			var props = new List<Property>();
			props.Add(ReadIndexItem());
			while (Peek(1) == ',')
			{
				Move(1);
				props.Add(ReadIndexItem());
			}
			return props.ToArray();
		}

		private char Peek(int count)
		{
			if (count > remaining)
				throw new ApplicationException("Unexpected end of pattern.");
			return source[position + count];
		}
		private void Move(int count)
		{
			if (count == 0) return;
			if (count > remaining)
				throw new ApplicationException("Unexpected end of pattern.");

			remaining -= count;
			position += count;
			current = source[position];
		}
		private bool IsValidFirstChar(char c)
		{
			return c.IsAsciiLetter() || c == '_';
		}
	}
}
