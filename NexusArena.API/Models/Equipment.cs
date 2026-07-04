#nullable disable

using System;
using System.Collections.Generic;

namespace NexusArena.API.Models;

public partial class Equipment
{
    public int EquipmentId { get; set; }

    public int ArenaId { get; set; }

    public int CategoryId { get; set; }

    public string ItemName { get; set; } = null!;

    public decimal PricePerItem { get; set; }

    public int TotalStock { get; set; }

    public virtual Arena Arena { get; set; } = null!;

    public virtual ICollection<BookingEquipment> BookingEquipments { get; set; } = new List<BookingEquipment>();

    public virtual SportCategory Category { get; set; } = null!;
}
