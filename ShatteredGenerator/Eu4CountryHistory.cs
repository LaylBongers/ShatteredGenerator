namespace ShatteredGenerator
{
	internal sealed class Eu4CountryHistory
	{
		private readonly Eu4FileData _data;

		public Eu4CountryHistory(Eu4FileData data)
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
	}
}