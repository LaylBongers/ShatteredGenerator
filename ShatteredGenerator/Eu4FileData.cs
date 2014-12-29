using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShatteredGenerator
{
	public class Eu4FileData
	{
		private List<KeyValuePair<string, string>> _entries;

		public Eu4FileData()
		{
			_entries = new List<KeyValuePair<string, string>>();
		}

		private Eu4FileData(List<KeyValuePair<string, string>> entries)
		{
			_entries = entries;
		}

		public Eu4FileData(string text)
			: this()
		{
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

				var makeLiteral =
					entry.Value.Contains(' ') ||
					entry.Value.Contains('\t') ||
					entry.Value.Contains('\n');

				// Make an exception if this starts with a {, because that's a nested object
				if (makeLiteral && entry.Value.StartsWith("{"))
				{
					makeLiteral = false;
				}

				var value = makeLiteral
					? "\"" + entry.Value + "\""
					: entry.Value;
				
				builder.AppendLine(value);
			}

			return builder.ToString();
		}

		public Eu4FileData Clone()
		{
			return new Eu4FileData(_entries.ToList());
		}

		public string One(string key)
		{
			return _entries.FirstOrDefault(e => e.Key == key).Value;
		}

		public IEnumerable<string> Many(string key)
		{
			return _entries.Where(e => e.Key == key).Select(e => e.Value);
		}

		public Eu4FileData OneNested(string key)
		{
			var text = One(key);
			return new Eu4FileData(text.Substring(1, text.Length - 2));
		}

		public IEnumerable<Eu4FileData> ManyNested(string key)
		{
			var texts = Many(key);
			return texts.Select(text => new Eu4FileData(text.Substring(1, text.Length - 1)));
		}

		public void Set(string key, string value)
		{
			// Remove the old
			_entries = _entries.Where(e => e.Key != key).ToList();

			// Add the new
			_entries.Add(new KeyValuePair<string, string>(key, value));
		}

		public void RemoveAll(Predicate<KeyValuePair<string, string>> match)
		{
			_entries.RemoveAll(match);
		}

		public IEnumerable<KeyValuePair<string, string>> ManyMatching(Func<KeyValuePair<string, string>, bool> predicate)
		{
			return _entries.Where(predicate);
		}

		public void Add(string key, string value)
		{
			_entries.Add(new KeyValuePair<string, string>(key, value));
		}
	}
}