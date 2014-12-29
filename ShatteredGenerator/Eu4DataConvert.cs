using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Irony.Parsing;

namespace ShatteredGenerator
{
	public static class Eu4DataConvert
	{
		private static readonly Eu4DataGrammar Grammar = new Eu4DataGrammar();
		private static readonly LanguageData Language = new LanguageData(Grammar);
		private static readonly Parser Parser = new Parser(Language);

		public static Eu4Data Deserialize(string text)
		{
			// Little hack to correct unclosed quotation marks since APPARENTLY that is valid in the EU4 parser
			var lines = text.Split(new[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries).Select(l =>
			{
				var amount = l.Count(c => c == '\"');
				return amount%2 == 0
					? l
					: l + "\"";
			});
			text = string.Join("\r\n", lines);

			var parseTree = Parser.Parse(text);

			// If we don't have any errors, we can parse the resulting tree
			if (!parseTree.HasErrors())
			{
				// Uncomment for debugging
				//throw new Exception(CreateTreeString(parseTree.Root));

				return ParseExpressions(parseTree.Root);
			}

			// We do have errors, so we need to throw them
			var exceptionMessage = parseTree.ParserMessages.Aggregate(
				"Error(s) while parsing: \n",
				(current, message) => current + string.Format(
					"  {0} ({1}) {2}",
					message.Location, message.Level, message.Message));
			throw new InvalidOperationException(exceptionMessage);
		}

		private static Eu4Data ParseExpressions(ParseTreeNode node)
		{
			Debug.Assert(node.Term == Grammar.Expressions);

			var data = new Eu4Data();
			var expressions = node.ChildNodes;

			foreach (var expression in expressions)
			{
				var entry = ParseExpression(expression);
				data.Add(entry.Key, entry.Value);
			}

			return data;
		}

		private static KeyValuePair<string, Eu4DataEntry> ParseExpression(ParseTreeNode node)
		{
			Debug.Assert(node.Term == Grammar.Expression);
			var actualExpression = node.ChildNodes.First();

			if (actualExpression.Term == Grammar.KeyValue)
				return ParseKeyValue(actualExpression);
			if (actualExpression.Term == Grammar.Value)
				return new KeyValuePair<string, Eu4DataEntry>("", ParseValue(actualExpression));

			throw new NotSupportedException("Unsupported node in expression.");
		}

		private static KeyValuePair<string, Eu4DataEntry> ParseKeyValue(ParseTreeNode node)
		{
			Debug.Assert(node.Term == Grammar.KeyValue);

			var nodes = node.ChildNodes.ToArray();
			return new KeyValuePair<string, Eu4DataEntry>(ParseKey(nodes[0]), ParseValue(nodes[2]));
		}

		private static string ParseKey(ParseTreeNode node)
		{
			Debug.Assert(node.Term == Grammar.Key);

			var actual = node.ChildNodes.First();
			if (actual.Term == Grammar.Word || actual.Term == Grammar.Literal)
			{
				// Trimming quotes is not needed
				return actual.Token.ValueString;
			}

			throw new NotSupportedException("Unsupported value type.");
		}

		private static Eu4DataEntry ParseValue(ParseTreeNode node)
		{
			Debug.Assert(node.Term == Grammar.Value);

			var actual = node.ChildNodes.First();
			if (actual.Term == Grammar.Word || actual.Term == Grammar.Literal)
			{
				// Trimming quotes is not needed
				return new Eu4DataEntry {String = actual.Token.ValueString};
			}
			if (actual.Term == Grammar.NestedObject)
			{
				// We basically start from the start within this
				return new Eu4DataEntry {Data = ParseExpressions(actual.ChildNodes.First())};
			}

			throw new NotSupportedException("Unsupported value type.");
		}

		private static string CreateTreeString(ParseTreeNode node, int level = 0)
		{
			var text = "";
			for (var i = 0; i < level; i++)
				text += "  ";

			text += node + "\n";

			return node.ChildNodes.Aggregate(text,
				(current, child) => current + CreateTreeString(child, level + 1));
		}
	}
}