using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Mono.Options
{
	public abstract class Option
	{
		static readonly char[] NameTerminator = new char[] { '=', ':' };

		public string Prototype { get; private set; }
		public string Description { get; private set; }
		public OptionValueType OptionValueType { get; private set; }
		public int MaxValueCount { get; private set; }

		internal string[] Names { get; private set; }
		internal string[] ValueSeparators { get; private set; }

		protected Option(string prototype, string description)
			: this(prototype, description, 1)
		{
		}

		protected Option(string prototype, string description, int maxValueCount)
		{
			if (prototype == null)
				throw new ArgumentNullException("prototype");
			if (prototype.Length == 0)
				throw new ArgumentException("Cannot be the empty string.", "prototype");
			if (maxValueCount < 0)
				throw new ArgumentOutOfRangeException("maxValueCount");

			this.Prototype = prototype;
			this.Names = prototype.Split('|');
			this.Description = description;
			this.MaxValueCount = maxValueCount;
			ParsePrototype();

			if (this.MaxValueCount == 0 && OptionValueType != OptionValueType.None)
				throw new ArgumentException(
					"Cannot provide maxValueCount of 0 for OptionValueType.Required or OptionValueType.Optional.",
					"maxValueCount");
			if (this.OptionValueType == OptionValueType.None && maxValueCount > 1)
				throw new ArgumentException(
					string.Format("Cannot provide maxValueCount of {0} for OptionValueType.None.", maxValueCount),
					"maxValueCount");
			if (Array.IndexOf(Names, "<>") >= 0 &&
			    ((Names.Length == 1 && this.OptionValueType != OptionValueType.None) ||
			     (Names.Length > 1 && this.MaxValueCount > 1)))
				throw new ArgumentException(
					"The default option handler '<>' cannot require values.",
					"prototype");
		}

		public string[] GetNames()
		{
			return (string[])Names.Clone();
		}

		public string[] GetValueSeparators()
		{
			if (ValueSeparators == null)
				return new string[0];
			return (string[])ValueSeparators.Clone();
		}

		protected static T Parse<T>(string value, OptionContext c)
		{
			Type tt = typeof(T);
			bool nullable = tt.IsValueType && tt.IsGenericType &&
			                !tt.IsGenericTypeDefinition &&
			                tt.GetGenericTypeDefinition() == typeof(Nullable<>);
			Type targetType = nullable ? tt.GetGenericArguments()[0] : typeof(T);
			TypeConverter conv = TypeDescriptor.GetConverter(targetType);
			T t = default(T);
			try
			{
				if (value != null)
					t = (T)conv.ConvertFromString(value);
			}
			catch (Exception e)
			{
				throw new OptionException(
					string.Format(
						c.OptionSet.MessageLocalizer("Could not convert string `{0}' to type {1} for option `{2}'."),
						value, targetType.Name, c.OptionName),
					c.OptionName, e);
			}
			return t;
		}

		private void ParsePrototype()
		{
			char type = '\0';
			List<string> seps = new List<string>();
			for (int i = 0; i < Names.Length; ++i)
			{
				string name = Names[i];
				if (name.Length == 0)
					throw new ArgumentException("Empty option names are not supported.", "prototype");

				int end = name.IndexOfAny(NameTerminator);
				if (end == -1)
					continue;
				Names[i] = name.Substring(0, end);
				if (type == '\0' || type == name[end])
					type = name[end];
				else
					throw new ArgumentException(
						string.Format("Conflicting option types: '{0}' vs. '{1}'.", type, name[end]),
						"prototype");
				AddSeparators(name, end, seps);
			}

			if (type == '\0')
				return;

			if (MaxValueCount <= 1 && seps.Count != 0)
				throw new ArgumentException(
					string.Format("Cannot provide key/value separators for Options taking {0} value(s).", MaxValueCount),
					"prototype");
			if (MaxValueCount > 1)
			{
				if (seps.Count == 0)
					this.ValueSeparators = new string[] { ":", "=" };
				else if (seps.Count == 1 && seps[0].Length == 0)
					this.ValueSeparators = null;
				else
					this.ValueSeparators = seps.ToArray();
			}

			this.OptionValueType = type == '=' ? OptionValueType.Required : OptionValueType.Optional;
		}

		private static void AddSeparators(string name, int end, ICollection<string> seps)
		{
			int start = -1;
			for (int i = end + 1; i < name.Length; ++i)
			{
				switch (name[i])
				{
					case '{':
						if (start != -1)
							throw new ArgumentException(
								string.Format("Ill-formed name/value separator found in \"{0}\".", name),
								"prototype");
						start = i + 1;
						break;
					case '}':
						if (start == -1)
							throw new ArgumentException(
								string.Format("Ill-formed name/value separator found in \"{0}\".", name),
								"prototype");
						seps.Add(name.Substring(start, i - start));
						start = -1;
						break;
					default:
						if (start == -1)
							seps.Add(name[i].ToString());
						break;
				}
			}
			if (start != -1)
				throw new ArgumentException(
					string.Format("Ill-formed name/value separator found in \"{0}\".", name),
					"prototype");
		}

		public void Invoke(OptionContext c)
		{
			OnParseComplete(c);
			c.OptionName = null;
			c.Option = null;
			c.OptionValues.Clear();
		}

		protected abstract void OnParseComplete(OptionContext c);

		public override string ToString()
		{
			return Prototype;
		}
	}
}