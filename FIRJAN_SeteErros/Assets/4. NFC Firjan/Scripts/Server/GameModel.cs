namespace _4._NFC_Firjan.Scripts.Server
{
	public class GameModel
	{
		public string nfcId { get; set; }
		public int gameId { get; set; }
		public int skill1 { get; set; }
		public int skill2 { get; set; }
		public int skill3 { get; set; }

		public override string ToString()
		{
			return Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
		}
	}
}