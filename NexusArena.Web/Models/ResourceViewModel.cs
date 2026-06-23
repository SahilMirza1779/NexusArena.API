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

        [Required(ErrorMessage = "Capacity is required")]
        public int Capacity { get; set; } 

        public string Dimensions { get; set; }

        public string IncludedEquipment { get; set; }

        public string Description { get; set; }

        public IFormFile? ResourceImage { get; set; }

        public bool IsActive { get; set; }
    }
}