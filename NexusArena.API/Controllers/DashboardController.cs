using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace NexusArena.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        [HttpGet("admin-data")]
        [Authorize(Roles = "SuperAdmin")]
        public IActionResult GetAdminDashboard()
        {
            return Ok(new { message = "Welcome SuperAdmin! Here is your total software revenue." });
        }

        [HttpGet("owner-data")]
        [Authorize(Roles = "Owner")]
        public IActionResult GetOwnerDahboard()
        {
            return Ok(new { message = "Welcome Owner! Here are today's turf bookings." });
        }

        [HttpGet("user-data")]
        [Authorize(Roles = "User")]
        public IActionResult GetUserDashboard()
        {
            return Ok(new { message = "Welcome Player! Here is your upcoming match schedule." });
        }
    }
}
