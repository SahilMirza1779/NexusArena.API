using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NexusArena.API.Models;

namespace NexusArena.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin")]
    public class AdminReportsController : ControllerBase
    {
        private readonly NexusArenaDbContext _context;

        public AdminReportsController(NexusArenaDbContext context)
        {
            _context = context;
        }

        [HttpGet("dashboard-stats")]
        public IActionResult GetDashboardStats()
        {
            var totalOwners = _context.Users.Count(u => u.Role.RoleName == "Owner");
            var totalArenas = _context.Arenas.Count();
            var activeArenas = _context.Arenas.Count(a => a.IsActive == true);
            var totalCategories = _context.SportCategories.Count();

            var stats = new
            {
                TotalOwners = totalOwners,
                TotalArenas = totalArenas,
                ActiveArenas = activeArenas,
                TotalCategories = totalCategories
            };

            return Ok(stats);
        }
    }
}
