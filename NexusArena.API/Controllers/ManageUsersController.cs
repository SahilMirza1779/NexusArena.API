using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusArena.API.Models;

namespace NexusArena.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ManageUsersController : ControllerBase
    {
        private readonly NexusArenaDbContext _context;

        public ManageUsersController(NexusArenaDbContext context)
        {
            _context = context;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users
                .Where(u => u.RoleId == 3)
                .Select(u => new
                {
                    Id = u.UserId,
                    Name = u.FullName,
                    Email = u.Email,
                    Phone = u.Phone ?? "N/A",
                    IsActive = u.IsActive ?? true,
                    TotalBooking = u.Bookings.Count()
                })
                .ToListAsync();

            return Ok(users);
        }

        [HttpPost("ToggleStatus/{id}")]
        public async Task<IActionResult> ToggleUserStatus(int id)
        {
            var users = await _context.Users.FindAsync(id);
            if (users == null)
            {
                return NotFound(new { message = "User not Found!" });
            }

            users.IsActive = !(users.IsActive ?? true);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Status updated successfully!" });
        }
    }
}
