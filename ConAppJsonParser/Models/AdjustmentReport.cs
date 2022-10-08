namespace ConAppJsonParser.Models;

public class AdjustmentReport
{
    public string InvoiceId { get; set; }
    public string PackageId { get; set; }
    public string CatalogItemId { get; set; }
    public int QtyBiotrack { get; set; }
    public int QtyCova { get; set; }
    public int NewQty { get; set; }
}
