using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace NexusArena.API.Models
{
    public class Arena
    {
        [Key]
        public int ArenaId { get; set; }
        public int OwnerId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public bool IsActive { get; set; }

        // --- YEH HAIN WO PROPERTIES JO TERE DOST KA CODE DHOONDH RAHA THA ---
        [JsonIgnore]
        public virtual User? Owner { get; set; }
        [JsonIgnore]
        public virtual ICollection<Equipment>? Equipment { get; set; }
        [JsonIgnore]
        public virtual ICollection<Resource>? Resources { get; set; }
        [JsonIgnore]
        public virtual ICollection<Review>? Reviews { get; set; }
    }
}