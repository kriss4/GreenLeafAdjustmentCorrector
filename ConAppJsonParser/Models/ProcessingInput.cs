namespace ConAppJsonParser.Models;

public class ProcessingInput
{
    public string LogFileLocation { get; set; }
    public string FailedAdjFile { get; set; }
    public string AvailabilityBaseUrl { get; set; }
    public string CatalogsBaseUrl { get; set; }
    public string InventoryAdjustmentBaseUrl { get; set; }
    public string BioTrackApi { get; set; }
    public string AccountsBaseUrl { get; set; }
    public string ClientID { get; set; }
    public string ClientSecret { get; set; }
    public int CompanyId { get; set; }
    public int LocationId { get; set; }

    public string LicenseNumber { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
}
