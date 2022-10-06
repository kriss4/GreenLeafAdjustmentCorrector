using System.Text.Json.Serialization;

namespace ConAppJsonParser.Models
{
    public class FailedAdjustmentItem
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        public int CompanyId { get; set; }
        public string PrintableId { get; set; }
        public string AdjustmentId { get; set; }
        public bool IsSuccessful { get; set; }
        public string ProcessorResponse { get; set; }
    

        //"id": "1ee82b8c-9df2-4ef0-8cab-315a1c36e7fb",
        //"CompanyId": 276215,
        //"PrintableId": "ADB6W5-2833",
        //"AdjustmentId": "6f55eb55-df5f-4222-acaa-dca2a8ff31bd",
        //"IsSuccessful": false,
        //"ProcessorResponse": "Item 2679146033430336 was not found in BioTrack."
    }
}
