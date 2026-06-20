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

        // 🌟 STEP 1: Upgraded API with Search and Area Filter
        [HttpGet("arenas")]
        public async Task<IActionResult> GetAllArenas([FromQuery] string? searchTerm, [FromQuery] string? area)
        {
            try
            {
                // Pehle sirf Active arenas uthao (Sahil ne approve kiye hue)
                var query = _context.Arenas.Where(a => a.IsActive == true).AsQueryable();

                // Agar Search bar me kuch type kiya hai
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var term = searchTerm.ToLower();
                    query = query.Where(a => a.Name.ToLower().Contains(term) || a.City.ToLower().Contains(term));
                }

                // Agar Dropdown se Area select kiya hai
                if (!string.IsNullOrWhiteSpace(area))
                {
                    var areaTerm = area.ToLower();
                    query = query.Where(a => !string.IsNullOrEmpty(a.Location) && a.Location.ToLower().Contains(areaTerm));
                }

                // Data ko format karke return karo
                var arenas = await query
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

        // Yeh aapka purana method as-it-is rahega
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