using System.Text.Json.Serialization;

namespace DhanAlgoTrading.Models.DhanApi
{
    public class WebSocketAuthRequestDto
    {
        [JsonPropertyName("LoginReq")]
        public LoginRequestPayload? LoginReq { get; set; }

        [JsonPropertyName("UserType")]
        public string UserType { get; set; } = "SELF";
    }
    public class LoginRequestPayload
    {
        [JsonPropertyName("MsgCode")]
        public int MsgCode { get; set; } = 42;

        [JsonPropertyName("ClientId")]
        public string? ClientId { get; set; }

        [JsonPropertyName("Token")]
        public string? Token { get; set; }
    }

}
