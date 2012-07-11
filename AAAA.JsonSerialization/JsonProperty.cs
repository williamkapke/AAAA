using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Linq.Expressions;
using System.Diagnostics;
using System.Collections;
using AAAA.PropertyExpressions;

namespace AAAA.JsonSerialization
{
	//returns the error message
	//public delegate string ParseErrorHandler(object origionalValue);
	//public delegate void ParseFailCallback(object value);

	//public enum EnumerableType
	//{
	//    None,
	//    Array,
	//    IEnumerableT,
	//    ICollectionT,
	//    IListT,
	//    ListT,
	//}

	public static class JsonProperty
	{
		public delegate string Error(object origionalValue);
		public static Error DefaultError { get { return defaultError ?? _defaultError; } set { defaultError = value; } }
		public static Error defaultError;
		private static readonly Error _defaultError = o => "This information is required";
	}
	public interface IJsonProperty<TModel>
	{
		string Name { get; }
		Func<TModel, object> Get { get; }
		Action<TModel, object> Set { get; }
		IJsonPropertyParser Parser { get; set; }
		JsonItemWriter Writer { get; set; }
		SerializeDirection DoNotSerialize { get; }
		string TestValue(object value);
	}
	public class JsonProperty<TValue>
	{
		public delegate string Test(TValue value);
		public virtual IJsonPropertyParser Parser { get; set; }
		public virtual JsonItemWriter Writer { get; set; }
	}

	[DebuggerDisplay("{Name} ({typeCode})")]
	public class JsonProperty<TModel, TValue> : JsonProperty<TValue>, IJsonProperty<TModel>
	{
		private TypeCode typeCode;
		public string Name { get; protected set; }
		public SerializeDirection DoNotSerialize { get; protected set; }

		public Func<TModel, TValue> Get { get; set; }
		Func<TModel, object> IJsonProperty<TModel>.Get
		{
			get
			{
				if (Get == null) return null;
				return m => Get(m);
			}
		}

		public Action<TModel, TValue> Set { get; set; }
		Action<TModel, object> IJsonProperty<TModel>.Set
		{
			get
			{
				if (Set == null) return null;
				return SetWrapper;
			}
		}

		public virtual Test Tester { get; set; }

		public JsonProperty(PropertyInfo pi)
		{
			Type type = pi.PropertyType;
			Name = pi.Name;

			var attr = pi.GetCustomAttributes(typeof(IDoNotSerialize), true);
			DoNotSerialize = SerializeDirection.None;
			if (attr.Length == 1)
				DoNotSerialize = ((IDoNotSerialize)attr[0]).Direction;

			if (pi.CanWrite) Set = pi.GetSetAction<TModel, TValue>();
			if (pi.CanRead) Get = pi.GetGetFunc<TModel, TValue>();

			typeCode = Type.GetTypeCode(type);
			if (typeCode != TypeCode.Object)
			{
				Parser = new JsonPropertyParser.Convertible<TValue>(typeCode, null);
				return;
			}

			//examine the object for the static Parse method, if it has it- use it.
			var customparser = type.GetParserDelegate("Parse");
			if (customparser != null)
			{
				Parser = new JsonPropertyParser.Custom(customparser);
				return;
			}
			else
			{
				Type listGenericType = null;
				if (type.IsArray)
				{
					type = type.GetElementType();
					listGenericType = typeof(JsonPropertyParser.Array<>);
				}
				else if (type.IsGenericType)
				{
					Type genericType = type.GetGenericTypeDefinition();
					if (typeof(JsonArray<>) == genericType || typeof(List<>) == genericType)
					{
						type = type.GetGenericArguments()[0];
						listGenericType = typeof(JsonPropertyParser.List<>);
					}
					//IEnumerable<T> or IList<T> or...?
					else if (type.IsInterface && typeof(IEnumerable).IsAssignableFrom(type))
					{
						var enumerableGenericType = typeof(IEnumerable<>);
						Type type2 = type;
						if (enumerableGenericType != genericType)
							type2 = FindGenericInterface(type, enumerableGenericType);

						//this could happen if the type implements IEnumerable but not IEnumerable<T>
						if (type2 == null) return;

						type = type2.GetGenericArguments()[0];
						listGenericType = typeof(JsonPropertyParser.List<>);
					}
				}

				if (listGenericType != null)
				{
					var sub = CreateSubParser(type);
					if (sub != null)
					{
						var genericType = listGenericType.MakeGenericType(new Type[] { type });
						Parser = (IJsonPropertyParser)Activator.CreateInstance(genericType, sub, null);
					}
				}
			}
		}
		public IJsonPropertyParser CreateSubParser(Type type, JsonProperty.Error fail = null)
		{
			TypeCode typeCode = Type.GetTypeCode(type);
			if (typeCode != TypeCode.Object)
				return new JsonPropertyParser.Convertible<TValue>(typeCode, null);

			var customparser = type.GetParserDelegate("Parse");
			if (customparser != null)
				return new JsonPropertyParser.Custom(customparser);

			return null;
		}

		private Type FindGenericInterface(Type type, Type ifaceType)
		{
			for (; type != null; type = type.BaseType)
			{
				Type[] interfaces = type.GetInterfaces();
				if (interfaces == null) continue;

				for (int i = 0; i < interfaces.Length; i++)
				{
					Type t = interfaces[i];
					if (t.GetGenericTypeDefinition() == ifaceType)
						return t;
					t = FindGenericInterface(t, ifaceType);
					if (t != null)
						return t;
				}
			}
			return null;
		}
		private ObjectParser GetParser(string method, Type type)
		{
			var parameters = new ParameterExpression[]{
				Expression.Parameter(typeof(object), "value"),
				Expression.Parameter(typeof(Propex), "targets"),
				Expression.Parameter(typeof(object).MakeByRefType(), "targets")
			};
			var methodcall = Expression.Call(Expression.Constant(this), method, new Type[] { type }, parameters);
			return Expression.Lambda<ObjectParser>(methodcall, parameters).Compile();
		}
		public void SetWrapper(TModel instance, object value)
		{
			if (typeCode != TypeCode.Empty && typeCode != TypeCode.Object)
			{
				value = Convert.ChangeType(value, typeCode);
			}
			Set(instance, (TValue)value);
		}

		public string TestValue(object value)
		{
			return Tester == null ? null : Tester((TValue)value);
		}
	}
}
