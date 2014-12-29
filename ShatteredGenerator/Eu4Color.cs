using System.Globalization;
using System.Linq;

namespace ShatteredGenerator
{
	public class Eu4Color
	{
		public Eu4Color(Eu4Data data)
		{
			var values = data.Many("").ToArray();
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

		public Eu4Data ToData()
		{
			var data = new Eu4Data();
			data.Add("", Red.ToString(CultureInfo.InvariantCulture));
			data.Add("", Green.ToString(CultureInfo.InvariantCulture));
			data.Add("", Blue.ToString(CultureInfo.InvariantCulture));
			return data;
		}
	}
}