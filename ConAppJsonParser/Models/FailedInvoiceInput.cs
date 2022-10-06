using System.Text.Json.Serialization;

namespace ConAppJsonParser.Models;

public class FailedInvoiceInput
    {
        [JsonPropertyName("invoiceId")]
        public string InvoiceId { get; set; }

        [JsonPropertyName("locationId")]
        public int LocationId { get; set; }

        [JsonPropertyName("receiptNumber")]
        public string ReceiptNumber { get; set; }

        [JsonPropertyName("createdDateUtc")]
        public DateTime CreatedDateUtc { get; set; }

        [JsonPropertyName("packageId")]
        public string PackageId { get; set; }

        [JsonPropertyName("catalogItemId")]
        public string CatalogItemId { get; set; }

        [JsonPropertyName("quantity")]
        public double Quantity { get; set; }

        [JsonPropertyName("response")]
        public string Response { get; set; }
    }
