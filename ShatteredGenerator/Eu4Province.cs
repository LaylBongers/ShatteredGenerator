using System.Linq;

namespace ShatteredGenerator
{
	internal sealed class Eu4Province
	{
		private readonly Eu4FileData _data;

		public Eu4Province(Eu4FileData data)
		{
			_data = data;
		}

		public string Culture
		{
			get { return _data.One("culture"); }
			set { _data.Set("culture", value); }
		}

		public string Owner
		{
			get { return _data.One("owner"); }
			set { _data.Set("owner", value); }
		}

		public int ClearHistory()
		{
			int ignoreMe;

			// All history entry keys start with a #
			var history = _data.ManyMatching(e => int.TryParse(
				new string(new[] {e.Key.First(), '\0'}), out ignoreMe)).ToList();

			_data.RemoveAll(history.Contains);

			return history.Count;
		}

		public string Serialize()
		{
			return _data.Serialize();
		}

		public void AddCore(string countryTag)
		{
			_data.Add("add_core", countryTag);
		}
	}
}