using System;

namespace ShatteredGenerator
{
	public class Eu4Color
	{
		public Eu4Color(string data)
		{
			var values = data.Split(new[] {' ', '{', '}'}, StringSplitOptions.RemoveEmptyEntries);
			Red = (byte) int.Parse(values[0]);
			Green = (byte) int.Parse(values[1]);
			Blue = (byte) int.Parse(values[2]);
		}

		public Eu4Color(int red, int green, int blue)
		{
			Red = (byte) red;
			Green = (byte) green;
			Blue = (byte) blue;
		}

		public byte Red { get; set; }
		public byte Green { get; set; }
		public byte Blue { get; set; }

		public string Serialize()
		{
			return "{ " + Red + " " + Green + " " + Blue + " }";
		}
	}
}