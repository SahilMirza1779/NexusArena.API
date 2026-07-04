using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusArena.API.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace NexusArena.API.Controllers
{
    public class AddStaffRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? BusinessName { get; set; }
    }

    public class UpdateStaffRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? BusinessName { get; set; }
    }

    [Authorize(Roles = "Owner")]
    [Route("api/[controller]")]
    [ApiController]
    public class StaffController : ControllerBase
    {
        private readonly NexusArenaDbContext _context;
        public StaffController(NexusArenaDbContext context) => _context = context;

        [HttpPost("add")]
        public async Task<IActionResult> AddStaff([FromBody] AddStaffRequest input)
        {
            try
            {
                if (await _context.Users.AnyAsync(u => u.Email == input.Email))
                    return BadRequest("Email already registered!");

                var staff = new User
                {
                    FullName = input.FullName,
                    Email = input.Email,
                    Phone = input.Phone,
                    PasswordHash = input.Password,
                    RoleId = 3,
                    IsActive = true,
                    BusinessName = input.BusinessName
                };

                _context.Users.Add(staff);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Staff successfully added!" });
            }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAllStaff()
        {
            try
            {
                var staffList = await _context.Users
                    .Where(u => u.RoleId == 3)
                    .Select(u => new {
                        u.UserId,
                        u.FullName,
                        u.Email,
                        u.Phone,
                        u.IsActive,
                        u.BusinessName
                    }).ToListAsync();

                return Ok(staffList);
            }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        [HttpGet("GetByEmail/{email}")]
        public async Task<IActionResult> GetStaffByEmail(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email && u.RoleId == 3);
            if (user == null) return NotFound("Staff not found");

            return Ok(new
            {
                user.UserId,
                user.FullName,
                user.Email,
                user.Phone,
                user.IsActive,
                user.BusinessName
            });
        }

        [HttpPut("update/{email}")]
        public async Task<IActionResult> UpdateStaff(string email, [FromBody] UpdateStaffRequest input)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email && u.RoleId == 3);
            if (user == null) return NotFound("Staff not found");

            user.FullName = input.FullName;
            user.Phone = input.Phone;
            user.BusinessName = input.BusinessName;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Staff updated successfully" });
        }

        [HttpDelete("Delete/{email}")]
        public async Task<IActionResult> DeleteStaff(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email && u.RoleId == 3);
            if (user == null) return NotFound("Staff not found");

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Staff deleted successfully" });
        }
    }
}