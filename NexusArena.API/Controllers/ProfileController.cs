using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusArena.Api.Models;
using NexusArena.API.Models;

namespace NexusArena.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly NexusArenaDbContext _context;

        public ProfileController(NexusArenaDbContext context)
        {
            _context = context;
        }

        // GET: api/Profile/GetUser/1
        [HttpGet("GetUser/{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound("User not found");
            }

            // Database se data nikal kar DTO mein daal rahe hain
            var profileData = new ProfileUpdateDto
            {
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.Phone // Yahan theek kar diya (user.Phone)
            };

            return Ok(profileData);
        }

        // PUT: api/Profile/UpdateProfile/1
        [HttpPut("UpdateProfile/{id}")]
        public async Task<IActionResult> UpdateProfile(int id, [FromBody] ProfileUpdateDto model)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { Message = "User not found." });
            }

            // Naya data database wale user mein update kar rahe hain
            // '!' lagane se null reference wali saari warnings khatam ho jayengi
            user.FullName = model.FullName!;
            user.Email = model.Email!;
            user.Phone = model.PhoneNumber; // Yahan bhi theek kar diya (user.Phone)

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Profile updated successfully!" });
        }
    }
}