using System.Text.Json.Serialization;

namespace ConAppJsonParser.Models
{
    public class AccessTokenResult
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        [JsonPropertyName("expires_in")]
        public int Expiry { get; set; }
    }
}
