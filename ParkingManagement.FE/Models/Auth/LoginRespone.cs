using System.Text.Json.Serialization;

namespace ParkingManagement.FE.Models.Auth
{
    public class LoginResponse
    {
        [JsonPropertyName("accountId")]
        public string AccountId { get; set; } = string.Empty;

        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("fullName")]
        public string FullName { get; set; } = string.Empty;

        [JsonPropertyName("relatedId")]
        public string? RelatedId { get; set; }

        [JsonPropertyName("token")]
        public string Token { get; set; } = string.Empty;

        [JsonPropertyName("expiresAt")]
        public DateTime ExpiresAt { get; set; }
    }
}