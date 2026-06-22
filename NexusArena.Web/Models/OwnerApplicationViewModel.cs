using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace NexusArena.Web.Models
{
    public class OwnerApplicationViewModel
    {
        public string Name { get; set; }
        public string ArenaName { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public List<IFormFile> Photos { get; set; } 
    }
}