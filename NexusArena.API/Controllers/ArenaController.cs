using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusArena.API.Models; 
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace NexusArena.API.Controllers
{
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

        private int GetOwnerId()
        {
            var claim = User.Claims.FirstOrDefault(c => c.Type == "UserId" || c.Type == ClaimTypes.NameIdentifier);
            return claim != null ? int.Parse(claim.Value) : 1;
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddArena([FromBody] NewArenaDto input)
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

        [Authorize(Roles = "SuperAdmin")]
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAllArenas()
        {
            try
            {
                var arenas = await _context.Arenas
                    .Include(a => a.Owner) 
                    .Select(a => new {
                        a.ArenaId,
                        a.Name,
                        OwnerName = a.Owner != null ? a.Owner.FullName : "No Owner", 
                        a.City,
                        a.IsActive
                    }).ToListAsync();

                return Ok(arenas);
            }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }
    }
}