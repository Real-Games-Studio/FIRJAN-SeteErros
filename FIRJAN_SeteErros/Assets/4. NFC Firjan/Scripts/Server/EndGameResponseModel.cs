namespace _4._NFC_Firjan.Scripts.Server
{
	public class EndGameResponseModel
	{
		
		public string nfcId { get; set; }
		public Attributes attributes { get; set; }
		
		public class Attributes
		{
			public int active_listening { get; set; }
			public int adaptability { get; set; }
			public int agility { get; set; }
			public int analytical_thinking { get; set; }
			public int communication { get; set; }
			public int continuous_learning { get; set; }
			public int creativity { get; set; }
			public int critical_thinking { get; set; }
			public int curiosity { get; set; }
			public int decision_making { get; set; }
			public int digital_literacy { get; set; }
			public int empathy { get; set; }
			public int leadership { get; set; }
			public int logical_reasoning { get; set; }
			public int problem_solving { get; set; }
			public int resilience { get; set; }
			public int self_awareness { get; set; }
		}
		
		public override string ToString()
		{
			return Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
		}


	}
}