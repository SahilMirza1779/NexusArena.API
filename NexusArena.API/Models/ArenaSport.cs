#nullable disable

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NexusArena.API.Models
{
    public class ArenaSport
    {
        [Key]
        public int ArenaSportId { get; set; }

        public int ArenaId { get; set; }
        public virtual Arena Arena { get; set; } = null!;

        public int CategoryId { get; set; }
        public virtual SportCategory SportCategory { get; set; } = null!;
    }
}