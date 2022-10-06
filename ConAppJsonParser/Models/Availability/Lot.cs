namespace ConAppJsonParser.Models.Availability;

public class Lot
{
    public string LotId { get; set; }

    public IEnumerable<Package> Packages { get; set; }
}
