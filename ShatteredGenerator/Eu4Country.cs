using System.Globalization;

namespace ShatteredGenerator
{
	internal sealed class Eu4Country
	{
		private readonly Eu4FileData _data;

		public Eu4Country(Eu4FileData data)
		{
			_data = data;
		}

		public int Capital
		{
			get { return int.Parse(_data.One("capital")); }
			set { _data.Set("capital", value.ToString(CultureInfo.InvariantCulture)); }
		}

		public string Serialize()
		{
			return _data.Serialize();
		}

		public Eu4Country Clone()
		{
			return new Eu4Country(_data.Clone());
		}
	}
}