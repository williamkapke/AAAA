using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Linq.Expressions;
using System.IO;
using System.IO.Compression;

namespace AAAA
{
	public static class StringExtensions
	{
		public static string Join(this List<string> values, string separator)
		{
			return String.Join(separator, values.ToArray());
		}
		public static string Join(this string[] values, string separator)
		{
			return String.Join(separator, values);
		}
		public static string[] Split(this string value, Regex regex)
		{
			return regex.Split(value);
		}
		public static string Replace(this string value, Regex regex, string replacement)
		{
			return regex.Replace(value, replacement);
		}
		public static string Remove(this string value, string regex)
		{
			return Regex.Replace(value, regex, "");
		}
		public static string Remove(this string value, string regex, RegexOptions options)
		{
			return Regex.Replace(value, regex, "", options);
		}
		public static string Substring(this string value, string startAfter)
		{
			var s = value.IndexOf(startAfter);
			if (s == -1) return null;
			return value.Substring(s + startAfter.Length);
		}
		public static string Substring(this string value, string startAfter, string endBefore)
		{
			var s = value.IndexOf(startAfter);
			if (s == -1) return null;
			s += startAfter.Length;
			var e = value.IndexOf(endBefore, s);
			if (e == -1) e = value.Length - 1;
			return value.Substring(s, e);
		}
		public static bool TryParse<T>(this string value, out T item)
		{
			item = default(T);
			TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
			try
			{
				item = (T)converter.ConvertFromInvariantString(value);
				return true;
			}
			catch
			{
				return false;
			}
		}
		public static bool TryParse(this string value, Type type, out object item)
		{
			item = null;
			TypeConverter converter = TypeDescriptor.GetConverter(type);
			try
			{
				item = converter.ConvertFromInvariantString(value);
				return true;
			}
			catch
			{
				return false;
			}
		}
		#region public static bool TryConvertTo(this string value, TypeCode typeCode, out object result)
		public static bool TryConvertTo(this string value, TypeCode typeCode, out object result)
		{
			if ((value == null) && (((typeCode == TypeCode.Empty) || (typeCode == TypeCode.String)) || (typeCode == TypeCode.Object)))
			{
				result = null;
				return true;
			}

			switch (typeCode)
			{
				case TypeCode.Empty:
					throw new InvalidCastException("Cannot convert to Empty");

				case TypeCode.DBNull:
					throw new InvalidCastException("Cannot convert to DBNull");

				case TypeCode.String:
				case TypeCode.Object:
					result = value;
					return true;

				case TypeCode.Boolean: return TryParseBool(value, out result);
				case TypeCode.Char: return TryParseChar(value, out result);
				case TypeCode.SByte: return TryParseSByte(value, out result);
				case TypeCode.Byte: return TryParseByte(value, out result);
				case TypeCode.Int16: return TryParseInt16(value, out result);
				case TypeCode.UInt16: return TryParseUInt16(value, out result);
				case TypeCode.Int32: return TryParseInt32(value, out result);
				case TypeCode.UInt32: return TryParseUInt32(value, out result);
				case TypeCode.Int64: return TryParseInt64(value, out result);
				case TypeCode.UInt64: return TryParseUInt64(value, out result);
				case TypeCode.Single: return TryParseSingle(value, out result);
				case TypeCode.Double: return TryParseDouble(value, out result);
				case TypeCode.Decimal: return TryParseDecimal(value, out result);
				case TypeCode.DateTime: return TryParseDateTime(value, out result);
			}
			throw new ArgumentException("Unknown TypeCode");
		}
		private static bool TryParseBool(string value, out object result)
		{
			bool r;
			bool success = bool.TryParse(value, out r);
			result = success ? (object)r : null;
			return success;
		}
		private static bool TryParseChar(string value, out object result)
		{
			char r;
			bool success = char.TryParse(value, out r);
			result = success ? (object)r : null;
			return success;
		}
		private static bool TryParseSByte(string value, out object result)
		{
			SByte r;
			bool success = SByte.TryParse(value, out r);
			result = success ? (object)r : null;
			return success;
		}
		private static bool TryParseByte(string value, out object result)
		{
			Byte r;
			bool success = Byte.TryParse(value, out r);
			result = success ? (object)r : null;
			return success;
		}
		private static bool TryParseInt16(string value, out object result)
		{
			Int16 r;
			bool success = Int16.TryParse(value, out r);
			result = success ? (object)r : null;
			return success;
		}
		private static bool TryParseUInt16(string value, out object result)
		{
			UInt16 r;
			bool success = UInt16.TryParse(value, out r);
			result = success ? (object)r : null;
			return success;
		}
		private static bool TryParseInt32(string value, out object result)
		{
			Int32 r;
			bool success = Int32.TryParse(value, out r);
			result = success ? (object)r : null;
			return success;
		}
		private static bool TryParseUInt32(string value, out object result)
		{
			UInt32 r;
			bool success = UInt32.TryParse(value, out r);
			result = success ? (object)r : null;
			return success;
		}
		private static bool TryParseInt64(string value, out object result)
		{
			Int64 r;
			bool success = Int64.TryParse(value, out r);
			result = success ? (object)r : null;
			return success;
		}
		private static bool TryParseUInt64(string value, out object result)
		{
			UInt64 r;
			bool success = UInt64.TryParse(value, out r);
			result = success ? (object)r : null;
			return success;
		}
		private static bool TryParseSingle(string value, out object result)
		{
			Single r;
			bool success = Single.TryParse(value, out r);
			result = success ? (object)r : null;
			return success;
		}
		private static bool TryParseDouble(string value, out object result)
		{
			Double r;
			bool success = Double.TryParse(value, out r);
			result = success ? (object)r : null;
			return success;
		}
		private static bool TryParseDecimal(string value, out object result)
		{
			Decimal r;
			bool success = Decimal.TryParse(value, out r);
			result = success ? (object)r : null;
			return success;
		}
		private static bool TryParseDateTime(string value, out object result)
		{
			DateTime r;
			bool success = DateTime.TryParse(value, out r);
			result = success ? (object)r : null;
			return success;
		}
		#endregion

		public static IEnumerable<T> ParseEnum<T>(this IEnumerable<string> values)
		{
			if (values == null)
				return null;

			int count = values.Count();
			if (count == 0)
				return null;

			Type type = typeof(T);
			object[] objects = new object[count];

			T[] columns = new T[count];

			return values.Select(v => (T)Enum.Parse(type, v, false));
		}
		/// <summary>
		///		Returns an alternate value for a string that is null or an empty string.
		/// </summary>
		public static string IfNullOrEmpty(this string item, string alternate)
		{
			return String.IsNullOrEmpty(item) ? alternate : item;
		}
		/// <summary>
		///		Indicates whether the string is null or an empty string.
		/// </summary>
		public static bool IsNullOrEmpty(this string item)
		{
			return String.IsNullOrEmpty(item);
		}
		/// <summary>
		///		Determines if the length of the
		///		string is shorter than the specified
		///		value.
		/// </summary>
		public static bool IsShorterThan(this string item, int maxlength)
		{
			if (maxlength < 0) throw new ArgumentOutOfRangeException("maxlength", maxlength, "Strings cannot have a negative length.");
			if (item == null) return true;//null is shorter than everything
			return (item.Length < maxlength);
		}
		/// <summary>
		///		Determines if the length of the
		///		string is longer than the specified
		///		value.
		/// </summary>
		public static bool IsLongerThan(this string item, int minlength)
		{
			if (minlength < 0) throw new ArgumentOutOfRangeException("minlength", minlength, "Strings cannot have a negative length.");
			if (item == null) return false;//null is not longer than anything
			return (item.Length > minlength);
		}
		/// <summary>
		///		Determines if the length of the
		///		string is between the specified
		///		values. Values are INCLUSIVE.
		/// </summary>
		public static bool LengthIsBetween(this string item, int minlength, int maxlength)
		{
			if (minlength < 0) throw new ArgumentOutOfRangeException("minlength", minlength, "Strings cannot have a negative length.");
			if (maxlength < 0) throw new ArgumentOutOfRangeException("maxlength", maxlength, "Strings cannot have a negative length.");
			if (item == null) return false;//null cannot be between anything
			return (item.Length >= minlength) && (item.Length <= maxlength);
		}
		public static bool IsMatch(this string item, string regex)
		{
			return IsMatch(item, regex, RegexOptions.None);
		}
		public static bool IsMatch(this string item, string regex, RegexOptions options)
		{
			return Regex.IsMatch(item, regex, options);
		}
		/// <summary>
		///		Tests if the string contains only the characters 0 - 9
		/// </summary>
		public static bool IsDigits(this string value)
		{
			//this was speed tested and found to be the fastest way
			for (int i = 0, l = value.Length; i < l; i++)
			{
				var c = value[i];
				if (c < '0' && c > '9') return false;
			}
			return true;
		}
	}
}
