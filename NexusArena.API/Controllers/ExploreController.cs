using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusArena.API.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

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

        [HttpGet("arenas")]
        public async Task<IActionResult> GetAllArenas([FromQuery] string? searchTerm, [FromQuery] string? area)
        {
            try
            {
                var query = _context.Arenas
                    .Where(a => a.IsActive == true)
                    .Include(a => a.Reviews)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var term = searchTerm.ToLower();
                    query = query.Where(a => (a.Name != null && a.Name.ToLower().Contains(term)) ||
                                             (a.City != null && a.City.ToLower().Contains(term)));
                }

                if (!string.IsNullOrWhiteSpace(area))
                {
                    var areaTerm = area.ToLower();
                    query = query.Where(a => a.Location != null && a.Location.ToLower().Contains(areaTerm));
                }

                var arenas = await query
                    .Select(a => new
                    {
                        ArenaId = a.ArenaId,
                        Name = a.Name,
                        Location = a.Location,
                        City = a.City,
                        AverageRating = a.Reviews.Any() ? Math.Round(a.Reviews.Average(r => r.Rating), 1) : 0.0,
                        TotalReviews = a.Reviews.Count()
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
                        // 🌟 THE FIX: Agar Category null hui toh crash nahi hoga, "General" likh dega
                        SportCategory = r.Category != null ? r.Category.Name : "General",
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