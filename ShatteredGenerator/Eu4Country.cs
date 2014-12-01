using System.Globalization;
using System.Linq;

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

		public Eu4Color Color
		{
			get { return new Eu4Color(_data.One("color")); }
			set { _data.Set("color", value.Serialize()); }
		}

		public string Serialize()
		{
			return _data.Serialize();
		}

		public Eu4Country Clone()
		{
			return new Eu4Country(_data.Clone());
		}

		public int ClearHistory()
		{
			int ignoreMe;

			// All history entry keys start with a #
			var history = _data.ManyMatching(e => int.TryParse(
				new string(new[] { e.Key.First(), '\0' }), out ignoreMe)).ToList();

			_data.RemoveAll(history.Contains);

			return history.Count;
		}
	}
}