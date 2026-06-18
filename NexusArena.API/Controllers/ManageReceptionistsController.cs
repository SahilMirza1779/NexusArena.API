using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusArena.API.Models;

namespace NexusArena.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ManageReceptionistsController : ControllerBase
    {
        public readonly NexusArenaDbContext _context;

        public ManageReceptionistsController(NexusArenaDbContext context)
        {
            _context = context;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAllReceptionists()
        {
            var receptionists = await _context.Users
                .Where(u => u.RoleId == 4)
                .Select(u => new
                {
                    Id = u.UserId,
                    Name = u.FullName,
                    Email = u.Email,
                    Phone = u.Phone ?? "N/A",
                    IsActive = u.IsActive ?? true,
                })
                .ToListAsync();
            return Ok(receptionists);
        }

        [HttpPost("ToggleStatus/{id}")]
        public async Task<IActionResult> ToggleReceptionistStatus(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "Receptionist not Found!" });
            }
            user.IsActive = !(user.IsActive ?? true);
            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Status updated successfully!" });
        }
    }
}
