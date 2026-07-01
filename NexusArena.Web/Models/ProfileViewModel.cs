using System.ComponentModel.DataAnnotations;

namespace NexusArena.Web.Models
{
    public class ProfileViewModel
    {
        public int UserId { get; set; }

        // 🌟 [Required] lagane se form khali submit nahi hoga aur UI pe error dikhega
        [Required(ErrorMessage = "Please enter your full name.")]
        public string? FullName { get; set; }

        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be exactly 10 digits.")]
        public string? PhoneNumber { get; set; }
    }
}