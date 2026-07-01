using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusArena.API.Models; // Make sure API ka 'A', 'P', 'I' capital ho
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace NexusArena.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // 🌟 Sirf login user access karega
    public class ProfileController : ControllerBase
    {
        private readonly NexusArenaDbContext _context;

        public ProfileController(NexusArenaDbContext context)
        {
            _context = context;
        }

        // GET: api/Profile/me (URL mein ID pass nahi karni)
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userIdString = User.Claims.FirstOrDefault(c => c.Type == "UserId" || c.Type == "id")?.Value;
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
                return Unauthorized(new { message = "Invalid Token." });

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound("User not found");

            var profileData = new ProfileUpdateDto
            {
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.Phone // Map Phone directly
            };

            return Ok(profileData);
        }

        // PUT: api/Profile/update
        [HttpPut("update")]
        public async Task<IActionResult> UpdateProfile([FromBody] ProfileUpdateDto model)
        {
            var userIdString = User.Claims.FirstOrDefault(c => c.Type == "UserId" || c.Type == "id")?.Value;
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
                return Unauthorized();

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound(new { Message = "User not found." });

            user.FullName = model.FullName ?? user.FullName;
            user.Email = model.Email ?? user.Email;
            user.Phone = model.PhoneNumber ?? user.Phone;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Profile updated successfully!" });
        }
    }

    // DTO Class Yahi Rakh Lein (Alag file ki zaroorat nahi)
    public class ProfileUpdateDto
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
    }
}