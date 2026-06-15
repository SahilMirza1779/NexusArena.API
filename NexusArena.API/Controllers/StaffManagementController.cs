using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusArena.API.Models;
using System.Security.Cryptography;
using System.Text;

namespace NexusArena.API.Controllers
{
    // Swagger ko clean rakhne ke liye sirf 4 zaroori fields wali class
    public class CreateStaffRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Phone { get; set; }
    }

    [Authorize(Roles = "Owner")]
    [Route("api/[controller]")]
    [ApiController]
    public class StaffManagementController : ControllerBase
    {     
        private readonly NexusArenaDbContext _context;
        public StaffManagementController(NexusArenaDbContext context) => _context = context; 

        [HttpPost("create-receptionist")]
        public async Task<IActionResult> CreateStaff([FromBody] CreateStaffRequest input)
        {
            if (input == null) return BadRequest("Invalid staff data.");

            // Check if email already exists
            var emailExists = await _context.Set<User>().AnyAsync(u => u.Email == input.Email);
            if (emailExists) return BadRequest("This email is already registered.");

            // Get 'Receptionist' role ID from DB
            var staffRole = await _context.Set<Role>().FirstOrDefaultAsync(r => r.RoleName == "Receptionist");
            if (staffRole == null) return BadRequest("Receptionist role does not exist in DB.");

            // Map Request data to User Model
            var staffUser = new User
            {
                FullName = input.FullName,
                Email = input.Email,
                Phone = input.Phone,
                RoleId = staffRole.RoleId,
                IsActive = true
            };

            // Password Hashing
            if (!string.IsNullOrEmpty(input.Password))
            {
                staffUser.PasswordHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(input.Password)));
            }

            _context.Set<User>().Add(staffUser);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Staff account for {staffUser.FullName} created successfully!" });
        }
    }
}