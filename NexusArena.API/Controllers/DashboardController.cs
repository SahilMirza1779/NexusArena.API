using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NexusArena.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        [HttpGet("superadmin-only")]
        [Authorize(Roles = "SuperAdmin")] 
        public IActionResult GetSuperAdminData()
        {
            return Ok(new
            {
                message = "Welcome, Superadmin! Your token and access are absolutely perfect.",
                role = "SuperAdmin"
            });
        }

        [HttpGet("owner-only")]
        [Authorize(Roles = "Owner")] 
        public IActionResult GetOwnerData()
        {
            return Ok(new
            {
                message = "Welcome, owner! You can manage your Turfers and Brookings.",
                role = "Owner"
            });
        }

        [HttpGet("receptionist-only")]
        [Authorize(Roles = "Receptionist")]
        public IActionResult GetReceptionistData()
        {
            return Ok(new
            {
                message = "Welcome Receptionist! Access to Walk-in bookings is verified.",
                role = "Receptionist"
            });
        }

        [HttpGet("user-only")]
        [Authorize(Roles = "User")] 
        public IActionResult GetUserData()
        {
            return Ok(new
            {
                message = "Welcome, user! You can search for and book new arenas.",
                role = "User"
            });
        }
    }
}