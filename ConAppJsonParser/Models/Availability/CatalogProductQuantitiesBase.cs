using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConAppJsonParser.Models.Availability;

public class CatalogProductQuantitiesBase
{
    public Guid Id { get; set; }

    public int CompanyId { get; set; }

    public int EntityId { get; set; }

    public bool? IsSerialized { get; set; }

    public bool? IsDropShippable { get; set; }

    public bool? IsLot { get; set; }

    public long QuantityInStock { get; set; }

    public long QuantityOnOrder { get; set; }

    public long QuantityTransferIn { get; set; }

    public long QuantityTransferOut { get; set; }

    public int UnitId { get; set; }
}
