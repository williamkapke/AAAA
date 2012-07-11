using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Collections;
using System.IO;
using AAAA.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace AAAA.Json
{
	[DebuggerDisplay("{ToJsonUnrestricted()}")]
	public abstract class JsonModel<TModel, T> : JsonModel<TModel>, IId<T>, IJsonObject, IJsonDiffable
		where T : IEquatable<T>
		where TModel : JsonModel<TModel, T>, new()
	{
		public JsonModel() { }
		public JsonModel(T id) { this.Id = id; }
		public virtual T Id { get; set; }

		public static implicit operator T(JsonModel<TModel, T> item)
		{
			return (item == null) ? default(T) : item.Id;
		}
		public static implicit operator Ref<TModel, T>(JsonModel<TModel, T> item)
		{
			return (item == null) ? null : new Ref<TModel, T>((TModel)item);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
	[DebuggerDisplay("{ToJsonUnrestricted()}")]
	public abstract class JsonModel<TModel> : IJsonObject where TModel : JsonModel<TModel>, new()
	{
		private object syncRoot = new Object();
		private static string[] keys;
		protected static readonly ReadOnlyCollection<IJsonProperty<TModel>> Properties;

		public int Count
		{
			get { return Properties.Count; }
		}
		bool ICollection.IsSynchronized
		{
			get { return false; }
		}
		object ICollection.SyncRoot
		{
			get { return syncRoot; }
		}
		public ICollection<string> GetKeys()
		{
			return keys ?? (keys = Properties.Select(pi => pi.Name).ToArray());
		}
		public object this[string name]
		{
			get
			{
				var prop = Properties.FirstOrDefault(pi => pi.Name == name);
				if (prop == null) return null;
				return prop.Get((TModel)this);
			}
			set
			{
				var prop = Properties.FirstOrDefault(pi => pi.Name == name);
				if (prop == null)
					throw new ArgumentOutOfRangeException("key", name, "Model does not have a property with this name.");
				prop.Set((TModel)this, value);
			}
		}
		public static string[] PropertyNames { get { return keys ?? (keys = Properties.Select(pi => pi.Name).ToArray()); } }

		static JsonModel()
		{
			var list = new List<IJsonProperty<TModel>>();
			Type tmodel = typeof(TModel);
			Type jsonPropertyType = typeof(JsonProperty<,>);

			foreach (var pi in tmodel.GetProperties(BindingFlags.Public | BindingFlags.Instance))
			{
				Type propertyType = pi.PropertyType;
				//ignore any properties created on this class
				if (pi.Name == "Item" || pi.Name == "Count" || pi.Name == "IsSynchronized" || pi.Name == "SyncRoot")
					continue;

				Type genericType = jsonPropertyType.MakeGenericType(new Type[] { tmodel, propertyType });
				var prop = (IJsonProperty<TModel>)Activator.CreateInstance(genericType, pi);
				if (pi.Name == "Id")
					list.Insert(0, prop);
				else list.Add(prop);
			}
			Properties = list.AsReadOnly();
		}
		public bool TryGetValue(string key, out object value)
		{
			value = null;
			var prop = Properties.FirstOrDefault(pi => pi.Name == key);
			if (prop == null) return false;
			value = prop.Get((TModel)this);
			return true;
		}
		public bool ContainsKey(string key)
		{
			return Properties.Any(pi => pi.Name == key);
		}
		public void CopyTo(Array array, int index)
		{
			throw new NotImplementedException();
		}
		public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
		{
			foreach (var prop in Properties)
			{
				var pair = new KeyValuePair<string, object>(prop.Name, prop.Get((TModel)this));
				yield return pair;
			}
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public override string ToString()
		{
			return this.ToJson();
		}
		public string ToJson(JsonWriterOptions options)
		{
			var sw = new System.IO.StringWriter();
			WriteTo(sw, options);
			return sw.ToString();
		}
		public string ToJson(bool formatted = false, char formatChar = '\t', int maxDepth = JsonWriter.MAX_DEPTH, bool ignoreDirectionRestrictions = false, Propex targets = null, IList<Action<object>> augmentors = null)
		{
			var options = new JsonWriterOptions(maxDepth, formatChar, formatted, ignoreDirectionRestrictions, targets, augmentors);
			return ToJson(options);
		}
		public string ToJsonUnrestricted(bool formatted = false)
		{
			return ToJson(new JsonWriterOptions(ignoreDirectionRestrictions: true, formatted: formatted));
		}
		//this gets called if the model is a sub item of another object
		//{
		//	"MyModel": #somevalue#
		//}
		public void Write(JsonWriter.Item iw)
		{
			iw.WriteObject(Write);
		}
		//writes the individual items of a JSON object: Like:
		//{
		//	"#someitem1#":#somevalue1#,
		//	"#someitem2#":#somevalue2#,
		//	...etc
		//}
		private void Write(JsonWriter.Object ow)
		{
			if (ow.ParentProperty.SubProperties == null)
				foreach (var prop in Properties)
				{
					if (!ow.IgnoreDirectionRestrictions && prop.DoNotSerialize.HasFlag(SerializeDirection.Out)) continue;
					if (prop.Get == null) continue;
					TModel m = (TModel)this;
					object value = prop.Get(m);
					var property = new Property(prop.Name);
					if (value != null && prop.Writer != null)
						ow.WriteValue(property, iw => prop.Writer(value, iw));
					else ow.WriteValue(property, value);
				}
			else
				foreach (var target in ow.ParentProperty.SubProperties)
				{
					var prop = GetProperty(target.Name);
					if (!ow.IgnoreDirectionRestrictions && prop.DoNotSerialize.HasFlag(SerializeDirection.Out))
						throw new InvalidOperationException("Target '" + target.Name + "' does not allow outbound serialization.");

					if (prop.Get == null)
					{
						if (!target.IsOptional)
							throw new InvalidOperationException("Required property '" + target.Name + "' does not have 'Get' assigned.");
						continue;
					}
					object value = prop.Get((TModel)this);
					if (prop.Writer != null)
						ow.WriteValue(target, iw => prop.Writer(value, iw));
					else ow.WriteValue(target, value);
				}
		}
		public void WriteTo(TextWriter writer, JsonWriterOptions options)
		{
			JsonWriter.Write(writer, false, this, options);
		}
		public Propex DiffAgainst(IJsonObject other, Propex targets)
		{
			if (targets == null)
				throw new ArgumentNullException("targets");

			if (other == null)
				throw new ArgumentNullException("other");

			var errors = new List<Property>();
			foreach (Property target in targets)
			{
				string name = target.Name;
				object value1;
				if (!TryGetValue(name, out value1))
					throw new ArgumentException("Property '" + name + "' does not exist in model.");

				object value2;
				if (!other.TryGetValue(name, out value2))
					errors.Add(new Property(name));

				Property result = CompareProperty(name, value1, value2, target.SubProperties);
				if (result != null)
					errors.Add(result);
			}
			return errors.Count > 0 ? new Propex(errors.ToArray()) : null;
		}
		protected virtual Property CompareProperty(string name, object value, object other, Propex subtargets)
		{
			return IJsonObjectExtensions.Compare(name, value, other, subtargets);
		}

		public static TModel Parse(IJsonObject source, Propex targets, out JsonErrorCollection errors)
		{
			errors = new JsonErrorCollection();
			if (source == null) return default(TModel);

			string name;
			TModel model = new TModel();
			foreach (var target in targets)
			{
				name = target.Name;
				var prop = GetProperty(name);
				if (prop.DoNotSerialize.HasFlag(SerializeDirection.In))
					throw new InvalidOperationException("Target '" + name + "' does not allow inbound serialization.");

				if (prop.Set == null)
					throw new InvalidOperationException("The target's property '" + name + "' does not have a 'Set' delegate assigned.");

				if (prop.Parser == null || prop.Parser.Parse == null)
					throw new InvalidOperationException("The target's property '" + name + "' does not have a 'Parse' delegate assigned.");

				var item = source[name];
				if (item == null)
				{
					if (!target.IsOptional)
						errors.Add(name, (prop.Parser.Fail ?? JsonProperty.DefaultError)(item));
				}
				else //parse
				{
					object parseResult;
					if (!prop.Parser.Parse(item, target.SubProperties, out parseResult))
					{
						if (parseResult == null)
							parseResult = (prop.Parser.Fail ?? JsonProperty.DefaultError)(item);
						errors.Add(name, parseResult);
					}
					else //test
					{
						var errorMessage = prop.TestValue(parseResult);
						if (errorMessage != null)
							errors.Add(name, errorMessage);
					}

					//If we have errors already, there is no point to adding to the model.
					if (errors.Count == 0)
						prop.Set(model, parseResult);
				}
			}
			if (errors.Count > 0)
			{
				return default(TModel);
			}
			return model;
		}
		//static ObjectParser delegate implementation
		public static bool Parse(object value, Propex targets, out object result)
		{
			result = null;
			var source = value as IJsonObject;
			if (source == null) return false;
			JsonErrorCollection errors;
			var model = Parse(source, targets, out errors);
			if (errors.Count > 0)
			{
				result = errors;
				return false;
			}
			result = model;
			return true;
		}
		public static IJsonProperty<TModel> GetProperty(string name)
		{
			for (int i = 0, length = Properties.Count; i < length; i++)
			{
				var prop = Properties[i];
				if (prop.Name != name) continue;
				return prop;
			}
			throw new ArgumentException("Target not in model: " + name, "target");
		}

		public static void Prop<T>(string name,
			Func<TModel, T>/*			*/ get = null,
			JsonProperty.Error/*		*/ fail = null,
			JsonProperty<T>.Test/*		*/ test = null,
			Action<TModel, T>/*			*/ set = null,
			ObjectParser parse = null,
			JsonItemWriter write = null)
		{
			if (get == null && fail == null && test == null && set == null && parse == null && write == null)
				return;

			for (int i = 0, length = Properties.Count; i < length; i++)
			{
				var prop = Properties[i];
				if (prop.Name != name) continue;
				var prop2 = ((JsonProperty<TModel, T>)prop);

				if (get != null) prop2.Get = get == None<T> ? null : get;
				if (test != null) prop2.Tester = test == None<T> ? null : test;
				if (set != null) prop2.Set = set == None<T> ? null : set;
				if (parse != null) prop2.Parser = new JsonPropertyParser.Custom(parse == None<T> ? null : parse);
				if (prop2.Parser != null) prop2.Parser.Fail = fail ?? JsonProperty.DefaultError;
				if (write != null) prop2.Writer = write == None ? null : write;
				break;
			}
		}
		protected static T None<T>(TModel model) { return default(T); }	//get
		protected static void None<T>(TModel model, T value) { }		//set
		protected static string None<T>(T value) { return null; }		//test,fail
		protected static bool None<T>(object value, Propex targets, out object result) { result = null; return false; } //parse
		protected static void None(object value, JsonWriter.Item iw) { } //write
	}
}
