using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusArena.API.Models;

namespace NexusArena.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArenaController : ControllerBase
    {
        private readonly NexusArenaDbContext _context;

        public ArenaController(NexusArenaDbContext context)
        {
            _context = context;
        }

        [HttpGet("GetAll")]
        public  async Task<IActionResult> GetAllArenas()
        {
            var arenas = await _context.Arenas
                .Select(a => new
                {
                    Id = a.ArenaId,
                    ArenaName = a.Name,
                    OwnerName = a.Owner.FullName,
                    City = a.City,
                    Status = a.IsActive == true ? "Active" : (a.IsActive == false ? "Pending" : "Unknown")
                })
                .ToListAsync();

            return Ok(arenas);
        }

        [HttpGet("GetDetails/{id}")]
        public async Task<IActionResult> GetArenaDetails(int id)
        {
            var arena = await _context.Arenas
                .Where(a => a.ArenaId == id)
                .Select(a => new
                {
                    Id = a.ArenaId,
                    ArenaName = a.Name,
                    Location = a.Location ?? "Not Specified", 
                    City = a.City,
                    OwnerName = a.Owner.FullName,
                    Status = a.IsActive == true ? "Active" : "Pending"
                })
                .FirstOrDefaultAsync();

            if (arena == null)
            {
                return NotFound(new { message = "Arena nahi mila!" });
            }

            return Ok(arena);
        }

        [HttpPost("Suspend/{id}")]
        public async Task<IActionResult> SuspendArena(int id)
        {
            var arena = await _context.Arenas.FindAsync(id);
            if (arena == null)
            {
                return NotFound(new { message = "Couldn't find the Arena!" });
            }

            arena.IsActive = false;
            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }
    }
}
