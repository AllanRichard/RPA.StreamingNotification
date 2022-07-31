using Newtonsoft.Json;

namespace RPA.StreamingNotification.Models
{
    public class Token
    {
        [JsonProperty("access_token")]
        public string? TokenId { get; set; }

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonProperty("token_type")]
        public string? TokenType { get; set; }
    }
}
