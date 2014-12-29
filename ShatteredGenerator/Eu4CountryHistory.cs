using System.Linq;

namespace ShatteredGenerator
{
	internal sealed class Eu4CountryHistory
	{
		private readonly Eu4Data _data;

		public Eu4CountryHistory(Eu4Data data)
		{
			_data = data;
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