using System.Text.Json.Serialization;

namespace DhanAlgoTrading.Api.Models.DhanApi.WebSocket
{
    public class LoginRequestPayload
    {
        [JsonPropertyName("MsgCode")]
        public int MsgCode { get; set; } = 42; // Default as per docs

        [JsonPropertyName("ClientId")]
        public string? ClientId { get; set; }

        [JsonPropertyName("Token")]
        public string? Token { get; set; }
    }

    public class WebSocketAuthRequestDto
    {
        [JsonPropertyName("LoginReq")]
        public LoginRequestPayload? LoginReq { get; set; }

        [JsonPropertyName("UserType")]
        public string UserType { get; set; } = "SELF"; // Default for individual traders
    }
}