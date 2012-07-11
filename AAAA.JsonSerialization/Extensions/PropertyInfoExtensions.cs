using System;
using System.Linq.Expressions;
using System.Reflection;

namespace AAAA.JsonSerialization
{
	public static class PropertyInfoExtensions
	{
		internal static Action<TInstance, T> GetSetAction<TInstance, T>(this PropertyInfo pi)
		{
			var param1 = Expression.Parameter(typeof(TInstance), "instance");
			var param2 = Expression.Parameter(typeof(T), "value");
			var prop = Expression.PropertyOrField(param1, pi.Name);
			var assign = Expression.Assign(prop, param2);
			return Expression.Lambda<Action<TInstance, T>>(assign, param1, param2).Compile();
		}
		internal static Func<TInstance, T> GetGetFunc<TInstance, T>(this PropertyInfo pi)
		{
			var param1 = Expression.Parameter(typeof(TInstance), "instance");
			var prop = Expression.PropertyOrField(param1, pi.Name);
			return Expression.Lambda<Func<TInstance, T>>(prop, param1).Compile();
		}
	}
}
