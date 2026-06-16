using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusArena.API.Models;

namespace NexusArena.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly NexusArenaDbContext _context;

        public DashboardController(NexusArenaDbContext context)
        {
            _context = context;
        }

        [HttpGet("Stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var totalPlayers = await _context.Users.CountAsync(u => u.RoleId == 3);
            var registeredOwners = await _context.Users.CountAsync(u => u.RoleId == 2);
            var totalReceptionists = await _context.Users.CountAsync(u => u.RoleId == 4);

            var activeArenas = await _context.Arenas.CountAsync(a => a.IsActive == true);
            string formattedRevenue = "₹0L";

            var stats = new
            {
                TotalPlayers = totalPlayers,
                RegisteredOwners = registeredOwners,
                TotalReceptionists = totalReceptionists,
                ActiveArenas = activeArenas,
                PlatformRevenue = formattedRevenue
            };

            return Ok(stats);
        }

        [HttpGet("PendingArenas")]
        public async Task<IActionResult> GetPendingArenas()
        {
            var pendingArenas = await _context.Arenas
                .Where(a => a.IsActive == false || a.IsActive == null)
                .Select(a => new
                {
                    Id = a.ArenaId, 
                    ArenaName = a.Name, 
                    OwnerName = a.Owner.FullName,

                    Category = "Not Specified",

                    Status = a.IsActive == true ? "Active" : "Pending"
                })
                .ToListAsync();

            return Ok(pendingArenas);
        }

        [HttpPost("ApproveArena/{id}")]
        public async Task<IActionResult> ApproveArena(int id)
        {
            var arena = await _context.Arenas.FindAsync(id);

            if (arena == null)
            {
                return NotFound(new { message = "Couldn't find the arena!" });
            }

            arena.IsActive = true;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Arena successfully approved!" });
        }
    }
}