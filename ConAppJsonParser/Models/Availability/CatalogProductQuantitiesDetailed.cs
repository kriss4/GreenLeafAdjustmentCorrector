namespace ConAppJsonParser.Models.Availability;

public class CatalogProductQuantitiesDetailed : CatalogProductQuantitiesBase
{
    public IEnumerable<string> SerialNumbers { get; set; }

    public IEnumerable<Lot> Lots { get; set; }
}
