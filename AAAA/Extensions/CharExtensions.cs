using System;

namespace AAAA
{
	public static class CharExtensions
	{
		public static bool IsAscii(this char c)
		{
			return (c <= '\x007f');
		}
		public static bool IsLatin1(this char c)
		{
			return (c <= '\x00ff');
		}
		public static bool IsWhitespace(this char c)
		{
			return c == ' ' || c == '\t' || c == '\r' || c == '\n';
		}
		public static bool IsBetween(this char c, char c1, char c2)
		{
			return c >= c1 && c <= c2;
		}

		//these are only here to provide symantics about their operation
		public static bool IsAsciiLetter(this char c)
		{
			return c.IsAscii() && c.Lower().IsBetween('a', 'z');
		}
		internal static char Lower(this char c)
		{
			return (char)(c | ' ');
		}
		public static bool IsDigit(this char c)
		{
			return c >= '0' && c <= '9';
		}
	}
}
