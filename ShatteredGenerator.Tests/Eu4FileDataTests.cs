using System;
using System.Linq;
using Xunit;

namespace ShatteredGenerator.Tests
{
	public class Eu4FileDataTests
	{
		[Fact]
		public void Constructor_OneField_Serializes()
		{
			// Arrange
			const string text = "blah=test";

			// Act
			var data = new Eu4FileData(text);

			// Assert
			Assert.Equal(1, data.Count);
			Assert.Equal("test", data.One("blah"));
		}

		[Fact]
		public void Constructor_TwoFields_Serializes()
		{
			// Arrange
			const string text = "blah=test\ntest=blah";

			// Act
			var data = new Eu4FileData(text);

			// Assert
			Assert.Equal(2, data.Count);
			Assert.Equal("test", data.One("blah"));
			Assert.Equal("blah", data.One("test"));
		}

		[Fact]
		public void Constructor_TwoFieldsWithEmptyLine_Serializes()
		{
			// Arrange
			const string text = "blah=test\n\ntest=blah";

			// Act
			var data = new Eu4FileData(text);

			// Assert
			Assert.Equal(2, data.Count);
			Assert.Equal("test", data.One("blah"));
			Assert.Equal("blah", data.One("test"));
		}

		[Fact]
		public void Constructor_TwoSameKeyFields_Serializes()
		{
			// Arrange
			const string text = "blah=test\n\nblah=testing";

			// Act
			var data = new Eu4FileData(text);

			// Assert
			var fields = data.Many("blah").ToList();
			Assert.Equal(2, fields.Count);
			Assert.Equal("test", fields[0]);
			Assert.Equal("testing", fields[1]);
		}

		[Fact]
		public void Constructor_NestedObject_Serializes()
		{
			// Arrange
			const string text = "blah={\nbluh=test bleh=blegh\nflargh=flemish}";

			// Act
			var data = new Eu4FileData(text);

			// Assert
			Assert.Equal(1, data.Count);
			var nested = data.OneNested("blah");
			Assert.Equal(3, nested.Count);
			Assert.Equal("test", nested.One("bluh"));
			Assert.Equal("blegh", nested.One("bleh"));
			Assert.Equal("flemish", nested.One("flargh"));
		}

		[Fact]
		public void Constructor_CommentBreakingValue_Serializes()
		{
			// Arrange
			const string text = "blah=test#blughablargh\ntest=blah";

			// Act
			var data = new Eu4FileData(text);

			// Assert
			Assert.Equal(2, data.Count);
			Assert.Equal("test", data.One("blah"));
			Assert.Equal("blah", data.One("test"));
		}

		[Fact]
		public void Constructor_WeirdSpacing_Serializes()
		{
			// Arrange
			const string text = "blah =   test\n test \t =blah ";

			// Act
			var data = new Eu4FileData(text);

			// Assert
			Assert.Equal(2, data.Count);
			Assert.Equal("test", data.One("blah"));
			Assert.Equal("blah", data.One("test"));
		}

		[Fact]
		public void Constructor_QuotedString_Serializes()
		{
			// Arrange
			const string text = "blah = \"this is a test\"";

			// Act
			var data = new Eu4FileData(text);

			// Assert
			Assert.Equal(1, data.Count);
			Assert.Equal("this is a test", data.One("blah"));
		}

		[Fact]
		public void Constructor_NestedObjectOpeningBracketOnNewLine_Serializes()
		{
			// Arrange
			const string text = "blah=\n{\nbluh=test bleh=blegh\nflargh=flemish}";

			// Act
			var data = new Eu4FileData(text);

			// Assert
			Assert.Equal(1, data.Count);
			var nested = data.OneNested("blah");
			Assert.Equal(3, nested.Count);
			Assert.Equal("test", nested.One("bluh"));
			Assert.Equal("blegh", nested.One("bleh"));
			Assert.Equal("flemish", nested.One("flargh"));
		}

		[Fact]
		public void Constructor_NestedObjectOpeningBracketWithWeirdThings_SerializesIntact()
		{
			// Arrange
			const string nestedString = "{\n\tbluh\n\ttest blah\n\tbleh\n}";
			const string text = "blah = " + nestedString;

			// Act
			var data = new Eu4FileData(text);

			// Assert
			Assert.Equal(1, data.Count);
			var nested = data.One("blah");
			Assert.Equal(nestedString, nested);

			// Also it might have "s, that's a no-no here
			var literal = data.Serialize();
			Assert.False(literal.Contains('"'), "Literal contains \"s, they're not expected here.");
		}


		[Fact]
		public void Constructor_NestedObjectInLiteralComment_SerializesWithoutRemovingComment()
		{
			// Arrange
			const string nestedString = "{test=\"I am test #whatever\"}";
			const string text = "blah = " + nestedString;

			// Act
			var data = new Eu4FileData(text);

			// Assert
			Assert.Equal(1, data.Count);
			var nested = data.One("blah");
			Assert.Equal(nestedString, nested);
		}

		[Fact]
		public void Serialize_OneField_Serializes()
		{
			// Arrange
			var data = new Eu4FileData();

			// Act
			data.Set("blah", "test");
			var result = new Eu4FileData(data.Serialize());

			// Assert
			Assert.Equal(1, result.Count);
			Assert.Equal("test", result.One("blah"));
		}

		[Fact]
		public void Serialize_StringWithSpaces_SerializesWithQuotes()
		{
			// Arrange
			var data = new Eu4FileData();

			// Act
			data.Set("blah", "this is a test");
			var result = new Eu4FileData(data.Serialize());

			// Assert
			Assert.Equal(1, result.Count);
			Assert.Equal("this is a test", result.One("blah"));
		}

		[Fact]
		public void Serialize_StringWithEnters_SerializesWithQuotes()
		{
			// Arrange
			var data = new Eu4FileData();

			// Act
			data.Set("blah", "this is\na test");
			var result = new Eu4FileData(data.Serialize());

			// Assert
			Assert.Equal(1, result.Count);
			Assert.Equal("this is\na test", result.One("blah"));
		}

		[Fact]
		public void Serialize_Nested_SerializesIntact()
		{
			// Arrange
			var data = new Eu4FileData();
			const string text = "{I am\n one \t wacky crazy\rnested thing is\na test}";

			// Act
			data.Set("blah", text);
			var result = data.Serialize();

			// Assert
			Assert.Contains(text, result);
			var serializedResult = new Eu4FileData(result);
			Assert.Equal(1, serializedResult.Count);
			Assert.Equal(text, serializedResult.One("blah"));
		}
	}
}