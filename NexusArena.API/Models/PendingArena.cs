#nullable enable

using System;
using System.Collections.Generic;

namespace NexusArena.API.Models;

public partial class PendingArena
{
    public int Id { get; set; }

    public string OwnerName { get; set; } = null!;

    public string ArenaName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Address { get; set; } = null!;

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public string? ImagePaths { get; set; }

    public string? Status { get; set; }

    public DateTime? AppliedOn { get; set; }
}
