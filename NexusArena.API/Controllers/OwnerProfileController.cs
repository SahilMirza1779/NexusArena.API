using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusArena.API.Models;
using System;
using System.Threading.Tasks;

namespace NexusArena.API.Controllers
{
    public class UpdateOwnerProfileRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
    }

    [Authorize(Roles = "Owner")]
    [Route("api/[controller]")]
    [ApiController]
    public class OwnerProfileController : ControllerBase
    {
        private readonly NexusArenaDbContext _context;
        public OwnerProfileController(NexusArenaDbContext context) => _context = context;

        [HttpGet("GetByEmail/{email}")]
        public async Task<IActionResult> GetProfile(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return NotFound("Owner not found");

            return Ok(new
            {
                user.FullName,
                user.Email,
                user.Phone,
                user.BusinessName
            });
        }

        [HttpPut("update/{email}")]
        public async Task<IActionResult> UpdateProfile(string email, [FromBody] UpdateOwnerProfileRequest input)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return NotFound("Owner not found");

            user.FullName = input.FullName;
            user.Phone = input.Phone;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Profile updated successfully" });
        }
    }
}