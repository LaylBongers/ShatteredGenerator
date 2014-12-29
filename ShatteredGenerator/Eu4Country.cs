using System.Globalization;
using System.Linq;

namespace ShatteredGenerator
{
	internal sealed class Eu4Country
	{
		private readonly Eu4Data _data;

		public Eu4Country(Eu4Data data)
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
			get { return new Eu4Color(_data.OneNested("color")); }
			set { _data.Set("color", value.ToData()); }
		}
		
		//Added this and Religion so we can modify them to match the province they own
	        public string Culture
	        {
				get { return _data.One("culture"); }
	            set { _data.Set("culture", value); }
	        }
	
	        public string Religion
	        {
				get { return _data.One("religion"); }
	            set { _data.Set("religion", value); }
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
