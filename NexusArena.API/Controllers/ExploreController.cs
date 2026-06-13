using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusArena.API.Models;

namespace NexusArena.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "User")]
    public class ExploreController : ControllerBase
    {
        private readonly NexusArenaDbContext _context;

        public ExploreController(NexusArenaDbContext context)
        {
            _context = context;
        }

        // Saare active complexes/arenas dikhane ke liye
        [HttpGet("arenas")]
        public async Task<IActionResult> GetAllArenas()
        {
            try
            {
                var arenas = await _context.Arenas
                    .Where(a => a.IsActive == true)
                    .Select(a => new
                    {
                        ArenaId = a.ArenaId,
                        Name = a.Name,
                        Location = a.Location,
                        City = a.City
                    })
                    .ToListAsync();

                if (!arenas.Any())
                    return NotFound(new { message = "Abhi koi arenas available nahi hain." });

                return Ok(new { message = "Arenas fetched successfully", data = arenas });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // Kisi ek arena ke andar ke turfs/tables dikhane ke liye
        [HttpGet("arena/{arenaId}/resources")]
        public async Task<IActionResult> GetArenaResources(int arenaId)
        {
            try
            {
                var resources = await _context.Resources
                    .Include(r => r.Category)
                    .Where(r => r.ArenaId == arenaId)
                    .Select(r => new
                    {
                        ResourceId = r.ResourceId,
                        ResourceName = r.ResourceName,
                        SportCategory = r.Category.Name,
                        Capacity = r.Capacity
                    })
                    .ToListAsync();

                if (!resources.Any())
                    return NotFound(new { message = "Is arena me abhi koi resources available nahi hain." });

                return Ok(new { message = "Resources fetched successfully", data = resources });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }
    }
}