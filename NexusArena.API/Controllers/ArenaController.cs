using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusArena.API.Models; // Ek hi baar rakha hai
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace NexusArena.API.Controllers
{
    // Naam change kar diya taaki duplicate ka error na aaye
    public class NewArenaDto
    {
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
    }

    [Authorize(Roles = "Owner")]
    [Route("api/[controller]")]
    [ApiController]
    public class ArenaController : ControllerBase
    {
        private readonly NexusArenaDbContext _context;
        public ArenaController(NexusArenaDbContext context) => _context = context;

        // Helper method to get Logged in Owner's ID from Token
        private int GetOwnerId()
        {
            var claim = User.Claims.FirstOrDefault(c => c.Type == "UserId" || c.Type == ClaimTypes.NameIdentifier);
            return claim != null ? int.Parse(claim.Value) : 1;
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddArena([FromBody] NewArenaDto input) // Yahan bhi naam update kiya
        {
            try
            {
                var arena = new Arena
                {
                    OwnerId = GetOwnerId(),
                    Name = input.Name,
                    Location = input.Location,
                    City = input.City,
                    IsActive = true
                };

                _context.Arenas.Add(arena);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Arena successfully added!" });
            }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        [HttpGet("GetMyArenas")]
        public async Task<IActionResult> GetMyArenas()
        {
            try
            {
                int ownerId = GetOwnerId();
                var arenas = await _context.Arenas
                    .Where(a => a.OwnerId == ownerId)
                    .Select(a => new {
                        a.ArenaId,
                        a.Name,
                        a.Location,
                        a.City,
                        a.IsActive
                    }).ToListAsync();

                return Ok(arenas);
            }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }
    }
}