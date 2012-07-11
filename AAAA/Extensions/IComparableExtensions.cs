using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AAAA
{
	public static class IComparableExtensions
	{
		/// <summary>
		///		Determines if the value of the
		///		item is between the specified
		///		values. Values are INCLUSIVE.
		/// </summary>
		public static bool IsBetween<T>(this T item, T min, T max) where T : struct, IComparable, IComparable<T>, IEquatable<T>
		{
			return item.CompareTo(min) >= 0 && item.CompareTo(max) <= 0;
		}
	}
}
