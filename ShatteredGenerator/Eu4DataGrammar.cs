using Irony.Parsing;

namespace ShatteredGenerator
{
	internal class Eu4DataGrammar : Grammar
	{
		public Eu4DataGrammar()
		{
			// Generic terminals
			// Anything but ' ', =, ", #, {, }, \n, \r and \t
			Word = new RegexBasedTerminal("Word", @"[^ =""#\{\}\n\r\t]+");
			Literal = new StringLiteral("Literal", "\"", StringOptions.AllowsAllEscapes | StringOptions.AllowsLineBreak);

			// Complex non-terminals
			Expression = new NonTerminal("Expression");
			Expressions = new NonTerminal("Expressions");
			KeyValue = new NonTerminal("KeyValue");
			Value = new NonTerminal("Value");
			NestedObject = new NonTerminal("NestedObject");

			Expression.Rule = KeyValue | Value;
			Expressions.Rule = MakeStarRule(Expressions, Expression);
			KeyValue.Rule = Word + ToTerm("=") + Value;
			Value.Rule = Literal | Word | NestedObject;
			NestedObject.Rule = ToTerm("{") + Expressions + ToTerm("}");

			// Set the non-terminal that represents the full file
			Root = Expressions;

			// Add comments to ignore
			var comment = new CommentTerminal("Comment", "#", "\n");
			NonGrammarTerminals.Add(comment);

			MarkPunctuation("{", "}");
			RegisterBracePair("{", "}");
		}

		public NonTerminal Expression { get; set; }
		public NonTerminal Expressions { get; set; }
		public NonTerminal KeyValue { get; set; }
		public NonTerminal Value { get; set; }
		public NonTerminal NestedObject { get; set; }

		public Terminal Word { get; set; }
		public StringLiteral Literal { get; set; }
	}
}