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
			// This parser was written quick n dirty, this entire thing was.
			// I just wanted to get to play shattered universalis again.
			// If this file format ends up being more complex than I expected,
			// this will possibly need a rewrite.

			var inComment = false;
			var inValue = false;
			var justSwitchedToValue = false;
			var currentText = "";
			var keyText = "";
			var nestCount = 0;

			var awaitingValue = false;
			var awaitingValueGotNewline = false;

			var inLiteral = false;
			var wasInLiteral = false;

			foreach (var ch in text)
			{
				// Awaiting value causes some weird stuff with newlines
				if (awaitingValue)
				{
					switch (ch)
					{
						case '{':
							awaitingValue = false;
							awaitingValueGotNewline = false;
							inValue = true;
							break;

						case '\n':
							awaitingValueGotNewline = true;
							continue;

						default:
							if (awaitingValueGotNewline)
							{
								_entries.Add(new KeyValuePair<string, string>(keyText, ""));
								keyText = "";
							}
							else
							{
								inValue = true;
								justSwitchedToValue = true;
							}
							awaitingValue = false;
							awaitingValueGotNewline = false;
							break;
					}
				}

				// Literals go before comments and nesting
				if (inLiteral)
				{
					currentText += ch;

					if (ch == '"')
					{
						inLiteral = false;
						wasInLiteral = true;
					}

					continue;
				}

				// Special case for in comments (even in nesting)
				if (inComment)
				{
					if (ch == '\n')
						inComment = false; // This falls through because of hack-y reasons
					else
						continue;
				}

				// Special case for nesting
				if (nestCount != 0)
				{
					switch (ch)
					{
						case '{':
							nestCount++;
							currentText += ch;
							break;

						case '}':
							nestCount--;
							currentText += ch;
							wasInLiteral = false;
							break;

						case '#':
							inComment = true;
							break;

						case '"':
							currentText += ch;
							inLiteral = true;
							break;

						default:
							currentText += ch;
							break;
					}

					continue;
				}

				switch (ch)
				{
					case '"':
						currentText += ch;
						inLiteral = true;
						break;

					case '{':
						nestCount++;
						currentText += ch;
						break;

					case '=':
						keyText = currentText;
						currentText = "";
						awaitingValue = true;
						break;

					case '\n':
						if (inValue)
						{
							if (wasInLiteral)
							{
								wasInLiteral = false;
								currentText = currentText.Substring(1, currentText.Length - 2);
							}

							_entries.Add(new KeyValuePair<string, string>(keyText, currentText));
						}
						keyText = "";
						currentText = "";
						inValue = false;
						break;

					case '#':
						// Comments are also a value break
						inComment = true;
						break;

					case ' ':
					case '\t':
					case '\r': // Screw \r
						// Kind of a hack, in key ignore, in value accept it as terminator but ignore as first
						if (inValue && !justSwitchedToValue)
						{
							if (wasInLiteral)
							{
								wasInLiteral = false;
								currentText = currentText.Substring(1, currentText.Length - 2);
							}

							_entries.Add(new KeyValuePair<string, string>(keyText, currentText));
							keyText = "";
							currentText = "";
							inValue = false;
						}
						break;

					default:
						justSwitchedToValue = false;
						currentText += ch;
						break;
				}
			}

			if (inValue)
			{
				if (wasInLiteral)
				{
					currentText = currentText.Substring(1, currentText.Length - 2);
				}

				_entries.Add(new KeyValuePair<string, string>(keyText, currentText));
			}
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