using System.Text.Json.Serialization;

namespace ConAppJsonParser.Models;

internal class BioTrackRequest
{
    [JsonPropertyName("API")]
    public string Api { get; } = "4.0";

    [JsonPropertyName("action")]
    public string Action { get; set; }

    [JsonPropertyName("active")]
    public string Active { get; set; }

    [JsonPropertyName("license_number")]
    public string LicenseNumber { get; set; }

    [JsonPropertyName("username")]
    public string UserName { get; set; }

    [JsonPropertyName("password")]
    public string Password { get; set; }

    [JsonPropertyName("training")]
    public string Training { get; set; }

    [JsonPropertyName("nosession")]
    public string NoSession { get; set; } = "1";

    [JsonPropertyName("data")]
    public List<AdjustmentData> Payload { get; set; } = new List<AdjustmentData>();
}