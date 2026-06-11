using System;
using System.Collections.Generic;

namespace NexusArena.API.Models;

public partial class SportCategory
{
    public int CategoryId { get; set; }

    public string Name { get; set; } = null!;

    public string Icon { get; set; } = null!;

    public virtual ICollection<Equipment> Equipment { get; set; } = new List<Equipment>();

    public virtual ICollection<Resource> Resources { get; set; } = new List<Resource>();
}
