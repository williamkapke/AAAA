using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AAAA.PropertyExpressions;

namespace AAAA.JsonSerialization
{
	public delegate bool ObjectParser(object value, Propex targets, out object result);

	public interface IJsonPropertyParser
	{
		JsonProperty.Error Fail { get; set; }
		ObjectParser Parse { get; }
	}

	public static class JsonPropertyParser
	{
		public class Custom : IJsonPropertyParser
		{
			public JsonProperty.Error Fail { get; set; }
			public ObjectParser Parse { get; protected set; }

			internal Custom() { }
			public Custom(ObjectParser parser)
			{
				Fail = JsonProperty.DefaultError;
				Parse = parser;
			}
		}
		internal class Convertible<T> : Custom
		{
			private TypeCode typeCode;

			public Convertible(TypeCode typeCode, JsonProperty.Error fail)
			{
				this.typeCode = typeCode;
				Parse = ParseItem;
			}
			private bool ParseItem(object value, Propex targets, out object result)
			{
				result = null;
				var valueTypeCode = ((IConvertible)value).GetTypeCode();

				if (valueTypeCode == typeCode)
				{
					result = (T)value;
					return true;
				}

				if (valueTypeCode == TypeCode.String)
				{
					string stringValue = value as string;
					if (stringValue.TryConvertTo(typeCode, out result))
						return true;

					result = (Fail ?? JsonProperty.DefaultError)(value);
					return false;
				}
				else
				{
					result = (T)Convert.ChangeType(value, typeCode);
					return true;
				}
			}
		}
		public class Array<T> : List<T>
		{
			public Array(IJsonPropertyParser subparser, JsonProperty.Error fail = null) : base(subparser, fail) { }

			protected override bool ParseItems(object value, Propex targets, out object result)
			{
				if (base.ParseItems(value, targets, out result))
				{
					result = ((System.Collections.Generic.List<T>)result).ToArray();
					return true;
				}
				return false;
			}
		}
		public class List<T> : Custom
		{
			public IJsonPropertyParser SubParser { get; private set; }

			public List() { }
			public List(IJsonPropertyParser subparser, JsonProperty.Error fail = null)
			{
				Fail = fail ?? JsonProperty.DefaultError;
				SubParser = subparser;
				Parse = ParseItems;
			}
			protected virtual bool ParseItems(object value, Propex targets, out object result)
			{
				result = null;
				var collection = value as IEnumerable<object>;
				if (collection == null) return false;

				var results = new JsonArray<T>();
				var errors = new System.Collections.Generic.List<object>();
				var defaultProperty = targets["-1"];
				int i = -1;
				foreach (IJsonObject item in collection)
				{
					i++;
					if (i < targets.Min)
					{
						results.Add(default(T));
						continue;
					}
					if (i > targets.Max) break;

					if (item == null)
					{
						errors.Add(Fail(item));
						continue;
					}

					var prop = targets[i.ToString()] ?? defaultProperty;
					if (prop == null)
						throw new InvalidOperationException("No Property specifed for item[" + i + "] and no default Property is specified.");

					object parseResult;
					if (!SubParser.Parse(item, prop.SubProperties, out parseResult))
					{
						if (parseResult == null)
							parseResult = Fail(item);
						errors.Add(parseResult);
					}
					else
					{
						//If we have errors already, there is no point to adding to the results list.
						//The lists will have the same number of items if we haven't had any errors yet
						if (errors.Count == results.Count)
							results.Add((T)parseResult);
						errors.Add(Undefined.Value);
					}
				}
				if (errors.Count == results.Count)
				{
					result = results;
					return true;
				}
				result = errors.ToArray();
				return false;
			}
		}
	}
}
