using System.Collections.Generic;
using System.Linq;

namespace ShatteredGenerator
{
	public class Eu4FileData
	{
		private List<KeyValuePair<string, string>> _entries;

		public Eu4FileData(string text)
		{
			// This parser was written quick n dirty, this entire thing was.
			// I just wanted to get to play shattered universalis again.
			// If this file format ends up being more complex than I expected,
			// this will possibly need a rewrite.

			_entries = new List<KeyValuePair<string, string>>();

			var inComment = false;
			var inValue = false;
			var justSwitchedToValue = false;
			var currentText = "";
			var keyText = "";
			var nestCount = 0;
			var inLiteral = false;

			foreach (var ch in text)
			{
				if (inLiteral)
				{
					currentText += ch;

					if (ch == '"')
					{
						inLiteral = false;
					}

					continue;
				}

				// Special case for in comments (even in nesting)
				if (inComment)
				{
					if (ch == '\n')
						inComment = false; // This fall through because of hack-y reasons
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
							break;

						case '}':
							nestCount--;
							break;

						case '#':
							inComment = true;
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
						break;

					case '=':
						keyText = currentText;
						currentText = "";
						inValue = true;
						justSwitchedToValue = true;
						break;

					case '\n':
						if (inValue)
						{
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
				_entries.Add(new KeyValuePair<string, string>(keyText, currentText));
			}
		}

		public List<KeyValuePair<string, string>> Entries
		{
			get { return _entries; }
			set { _entries = Entries; }
		}

		public int Count
		{
			get { return _entries.Count; }
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
			return new Eu4FileData(text.Substring(1, text.Length - 1));
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
	}
}