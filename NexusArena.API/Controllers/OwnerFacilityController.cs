using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusArena.API.Models; // Apne Models ka namespace

namespace NexusArena.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Owner")]
    public class OwnerFacilityController : ControllerBase
    {
        private readonly NexusArenaDbContext _context;
        public OwnerFacilityController(NexusArenaDbContext context)
        {
            _context = context;
        }

        // 1. DATA ADD KARNE WALA METHOD
        [HttpPost("AddFacility")]
        public async Task<IActionResult> AddFacility([FromBody] Resource model)
        {
            if (model == null) return BadRequest("Invalid data.");

            // Database me save karo
            _context.Resources.Add(model);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Nayi facility successfully add ho gayi!" });
        }

        // 2. DATABASE SE DATA FETCH KARNE WALA METHOD
        [HttpGet("GetAllFacilities")]
        public async Task<IActionResult> GetAllFacilities()
        {
            // 🔥 Ab ye dummy data nahi, asli database se fetch karega!
            var list = await _context.Resources.ToListAsync();

            return Ok(list);
        }
    }
}