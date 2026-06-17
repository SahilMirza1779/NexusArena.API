using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusArena.API.Models; 
using System.Threading.Tasks;
using System.Linq;

namespace NexusArena.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ManageOwnersController : ControllerBase
    {
        private readonly NexusArenaDbContext _context;

        public ManageOwnersController(NexusArenaDbContext context)
        {
            _context = context;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAllOwners()
        {
            var owners = await _context.Users
                .Where(u => u.RoleId == 2)
                .Select(u => new
                {
                    Id = u.UserId,
                    Name = u.FullName,
                    Email = u.Email,
                    Phone = u.Phone ?? "N/A",
                    IsActive = u.IsActive ?? true,
                    TotalArenas = u.Arenas.Count()
                })
                .ToListAsync();

            return Ok(owners);
        }

        [HttpPost("ToggleStatus/{id}")]
        public async Task<IActionResult> ToggleOwnerStatus(int id)
        {
            var owner = await _context.Users.FindAsync(id);

            if (owner == null)
            {
                return NotFound(new { message = "Couldn't find the owner!" });
            }

            owner.IsActive = !(owner.IsActive ?? true);

            await _context.SaveChangesAsync();

            string statusMessage = owner.IsActive == true ? "Unblocked" : "Blocked";
            return Ok(new { success = true, message = $"Owner successfully {statusMessage}!" });
        }
    }
}