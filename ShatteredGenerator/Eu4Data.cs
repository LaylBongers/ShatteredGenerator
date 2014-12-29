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
				builder.Append(entry.Key);
				builder.Append("=");

				var value = entry.Value.String;
				var makeLiteral =
					value.Contains(' ') ||
					value.Contains('\t') ||
					value.Contains('\n');

				// Make an exception if this starts with a {, because that's a nested object
				// TODO: Instead make this pick up the Eu4Data object
				if (makeLiteral && value.StartsWith("{"))
				{
					makeLiteral = false;
				}

				value = makeLiteral
					? "\"" + value + "\""
					: value;

				builder.AppendLine(value);
			}

			return builder.ToString();
		}

		public Eu4Data Clone()
		{
			return new Eu4Data(_entries.ToList());
		}

		public string One(string key)
		{
			return _entries.FirstOrDefault(e => e.Key == key).Value.String;
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