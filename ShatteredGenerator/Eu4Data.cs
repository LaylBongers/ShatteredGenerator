using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShatteredGenerator
{
	public class Eu4Data
	{
		private List<KeyValuePair<string, Eu4DataEntry>> _entries;

		public Eu4Data()
		{
			_entries = new List<KeyValuePair<string, Eu4DataEntry>>();
		}

		private Eu4Data(List<KeyValuePair<string, Eu4DataEntry>> entries)
		{
			_entries = entries;
		}

		public int Count
		{
			get { return _entries.Count; }
		}

		public string Serialize()
		{
			var builder = new StringBuilder();
			foreach (var entry in _entries)
			{
				var key = entry.Key;

				// Make sure the key's in quotes if needed
				var makeKeyLiteral =
					key.Contains(' ') ||
					key.Contains('\t') ||
					key.Contains('\n');

				key = makeKeyLiteral
					? "\"" + key + "\""
					: key;

				builder.Append(key);
				builder.Append("=");

				if (entry.Value.String != null) // If we're dealing with a string value
				{
					var value = entry.Value.String;

					// Make sure the value's in quotes if needed
					var makeValueLiteral =
						value.Contains(' ') ||
						value.Contains('\t') ||
						value.Contains('\n');

					value = makeValueLiteral
						? "\"" + value + "\""
						: value;

					builder.AppendLine(value);
				}
				else if (entry.Value.Data != null) // If we're dealing with a nested object value
				{
					builder.AppendLine("{");
					builder.AppendLine(entry.Value.Data.Serialize());
					builder.AppendLine("}");
				}
			}

			return builder.ToString();
		}

		public Eu4Data Clone()
		{
			return new Eu4Data(_entries.ToList());
		}

		public string One(string key)
		{
			var entry = _entries.FirstOrDefault(e => e.Key == key);
			return entry.Value == null
				? null
				: entry.Value.String;
		}

		public IEnumerable<string> Many(string key)
		{
			return _entries.Where(e => e.Key == key).Select(e => e.Value.String);
		}

		public Eu4Data OneNested(string key)
		{
			return _entries.FirstOrDefault(e => e.Key == key).Value.Data;
		}

		public IEnumerable<Eu4Data> ManyNested(string key)
		{
			return _entries.Where(e => e.Key == key).Select(e => e.Value.Data);
		}

		public void Set(string key, string value)
		{
			// Remove the old
			_entries = _entries.Where(e => e.Key != key).ToList();

			// Add the new
			_entries.Add(new KeyValuePair<string, Eu4DataEntry>(key, new Eu4DataEntry {String = value}));
		}

		public void RemoveAll(Predicate<KeyValuePair<string, Eu4DataEntry>> match)
		{
			_entries.RemoveAll(match);
		}

		public IEnumerable<KeyValuePair<string, Eu4DataEntry>> ManyMatching(
			Func<KeyValuePair<string, Eu4DataEntry>, bool> predicate)
		{
			return _entries.Where(predicate);
		}

		public void Add(string key, string value)
		{
			_entries.Add(new KeyValuePair<string, Eu4DataEntry>(key, new Eu4DataEntry {String = value}));
		}

		public void Add(string key, Eu4DataEntry entry)
		{
			_entries.Add(new KeyValuePair<string, Eu4DataEntry>(key, entry));
		}
	}
}