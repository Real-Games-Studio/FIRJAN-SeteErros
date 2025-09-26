namespace _4._NFC_Firjan.Scripts.Server
{
	public class EndGameRequestModel
	{
		public string name { get; set; }

		public override string ToString()
		{
			return Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
		}
	}
}