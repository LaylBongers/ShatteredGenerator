using Irony.Parsing;

namespace ShatteredGenerator
{
	internal class Eu4DataGrammar : Grammar
	{
		public Eu4DataGrammar()
		{
			// Generic terminals
			// Anything but ' ', =, ", \n, \r and \t
			Word = new RegexBasedTerminal("Word", "[^ =\"\n\r\t]+");
			Literal = TerminalFactory.CreateCSharpString("Literal");

			// Complex non-terminals
			Expression = new NonTerminal("Expression");
			Expressions = new NonTerminal("Expressions");
			KeyValue = new NonTerminal("KeyValue");
			Value = new NonTerminal("Value");

			Expression.Rule = KeyValue | Value;
			Expressions.Rule = MakeStarRule(Expressions, Expression);
			KeyValue.Rule = Word + ToTerm("=") + Value;
			Value.Rule = Literal | Word;

			// Set the non-terminal that represents the full file
			Root = Expressions;

			MarkPunctuation("{", "}");
		}

		public NonTerminal Expression { get; set; }
		public NonTerminal Expressions { get; set; }
		public NonTerminal KeyValue { get; set; }
		public NonTerminal Value { get; set; }

		public Terminal Word { get; set; }
		public Terminal Literal { get; set; }
	}
}