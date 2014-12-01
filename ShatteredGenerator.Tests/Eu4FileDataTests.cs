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
			Assert.Equal("\"this is a test\"", data.One("blah"));
		}
	}
}