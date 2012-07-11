using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.IO;

namespace AAAA
{
	public static class IEnumerableExtensions
	{
		/// <summary>
		///		Determines if the length of the array
		///		is between the specified values. Values are INCLUSIVE.
		/// </summary>
		public static bool CountIsBetween<T>(this IEnumerable<T> items, int min, int max)
		{
			int count = items.Count();
			return count >= min && count <= max;
		}
		public static void Each<T>(this IEnumerable<T> items, Action<T> action)
		{
			foreach (T item in items) action(item);
		}
	}
}
