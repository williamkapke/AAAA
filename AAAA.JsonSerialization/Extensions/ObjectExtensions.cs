using System;

namespace AAAA
{
	public static class ObjectExtensions
	{
		public static bool TryAs<TResult>(this Object obj, Action<TResult> action) where TResult : class
		{
			TResult r = obj as TResult;
			if (r == null) return false;
			action(r);
			return true;
		}
	}
}
