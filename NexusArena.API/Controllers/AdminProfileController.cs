using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusArena.API.Models;
using System.Threading.Tasks;

namespace NexusArena.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminProfileController : ControllerBase
    {
        private readonly NexusArenaDbContext _context;

        public AdminProfileController(NexusArenaDbContext context)
        {
            _context = context;
        }

        public class ProfileDto
        {
            public string FullName { get; set; }
            public string DisplayName { get; set; }
            public string Email { get; set; }
            public string Phone { get; set; }
            public string Location { get; set; }
            public string Role { get; set; }
        }

        [HttpGet("GetAdmin/{id}")]
        public async Task<IActionResult> GetAdmin(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound(new { message = "Admin record not found" });

            var profile = new ProfileDto
            {
                FullName = user.FullName ?? "Super Admin", 
                DisplayName = user.FullName ?? "Admin",
                Email = user.Email ?? "",
                Phone = user.Phone ?? "N/A",
                Location = "Surat, Gujarat, India",
                Role = "Super Admin & Founder"
            };

            return Ok(profile);
        }

        [HttpPut("UpdateAdmin/{id}")]
        public async Task<IActionResult> UpdateAdmin(int id, [FromBody] ProfileDto model)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.FullName = model.FullName;
            user.Email = model.Email;
            user.Phone = model.Phone;

            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }
    }
}