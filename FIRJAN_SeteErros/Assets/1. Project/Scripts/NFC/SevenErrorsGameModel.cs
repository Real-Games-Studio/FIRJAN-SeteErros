using Newtonsoft.Json;

namespace _4._NFC_Firjan.Scripts.Server
{
    /// <summary>
    /// Modelo para envio das habilidades do Jogo dos 7 Erros com nomes das habilidades
    /// </summary>
    public class SevenErrorsGameModel
    {
        [JsonProperty("nfcId")]
        public string nfcId { get; set; }

        [JsonProperty("gameId")]
        public int gameId { get; set; }

        [JsonProperty("skill1")]
        public int skill1 { get; set; }

        [JsonProperty("skill2")]
        public int skill2 { get; set; }

        [JsonProperty("skill3")]
        public int skill3 { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}