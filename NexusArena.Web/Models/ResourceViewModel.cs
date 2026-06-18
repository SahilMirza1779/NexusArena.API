using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace NexusArena.MVC.Models
{
    public class ResourceViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Resource Name is required")]
        public string ResourceName { get; set; }

        public string ResourceType { get; set; }

        [Required(ErrorMessage = "Price is required")]
        public decimal BasePricePerHour { get; set; }

        // Nayi Fields Niche Hain 👇

        [Required(ErrorMessage = "Capacity is required")]
        public int Capacity { get; set; } // Kitne log khel sakte hain

        public string Dimensions { get; set; } // e.g., 5v5, 8ft Table

        public string IncludedEquipment { get; set; } // e.g., 2 Bats, 1 Ball

        public string Description { get; set; }

        // Image upload ke liye IFormFile use hota hai
        public IFormFile? ResourceImage { get; set; }

        public bool IsActive { get; set; }
    }
}