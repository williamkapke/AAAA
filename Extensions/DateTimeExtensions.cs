using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AAAA
{
	public static class DateTimeExtensions
	{
		/// <summary>
		///		Determines if the value of the
		///		DateTime is before the specified
		///		value. Value is NON-INCLUSIVE.
		/// </summary>
		public static bool IsBefore(this DateTime item, DateTime datetime)
		{
			return (item < datetime);
		}
		/// <summary>
		///		Determines if the value of the
		///		DateTime is after the specified
		///		value. Value is NON-INCLUSIVE.
		/// </summary>
		public static bool IsAfter(this DateTime item, DateTime datetime)
		{
			return (item > datetime);
		}
		/// <summary>
		///		Determines if the value of the
		///		DateTime is between the specified
		///		values. Values are INCLUSIVE.
		/// </summary>
		public static bool IsBetween(this DateTime item, DateTime mindate, DateTime maxdate)
		{
			return (item >= mindate) && (item <= maxdate);
		}
	}
}
