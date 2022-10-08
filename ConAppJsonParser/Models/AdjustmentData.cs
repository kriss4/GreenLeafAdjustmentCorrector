using System.Text.Json.Serialization;

namespace ConAppJsonParser.Models;

public class AdjustmentData
{
    [JsonPropertyName("barcodeid")]
    public string BarcodeId { get; set; } //"barcodeid": "7842825485478757",
    
    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }     //  "quantity": "8",

    [JsonPropertyName("reason")]
    public string Reason { get; set; } = "Correcting a mistake";  //   "reason": "Theft",

    [JsonPropertyName("type")]
    public string Type { get; set; } = "4"; //  "type": "2"
}
