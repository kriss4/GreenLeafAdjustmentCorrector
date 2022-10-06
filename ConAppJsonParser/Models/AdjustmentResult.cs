using System.Text.Json.Serialization;

namespace ConAppJsonParser.Models;

public class AdjustmentResult
{
    [JsonPropertyName("sessiontime")]
    public string Sessiontime { get; set; }

    [JsonPropertyName("transactionid")]
    public string Transactionid { get; set; }

    [JsonPropertyName("success")]
    public int Success { get; set; }
}
