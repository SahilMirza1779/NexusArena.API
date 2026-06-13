using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusArena.API.Models;

namespace NexusArena.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin")]
    public class SuperAdminController : ControllerBase
    {
        private readonly NexusArenaDbContext _context;

        public SuperAdminController(NexusArenaDbContext context)
        {
            _context = context;
        }

        public class CreateOwnerRequest
        {
            public string FullName { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public string PhoneNumber { get; set; } = string.Empty;
        }

        [HttpPost("create-owner")]
        public IActionResult CreateOwner([FromBody] CreateOwnerRequest request)
        {
            if (_context.Users.Any(u => u.Email == request.Email))
            {
                return BadRequest(new { message = "This email is already registered with another account!" });
            }

            var ownerRole = _context.Roles.FirstOrDefault(r => r.RoleName == "Owner");
            if (ownerRole == null)
            {
                return StatusCode(500, new { message = "System error: Owner role not found in database." });
            }

            var newOwner = new User
            {
                RoleId = ownerRole.RoleId,
                FullName = request.FullName,
                Email = request.Email,
                PasswordHash = request.Password,
                Phone = request.PhoneNumber,
                IsActive = true
            };

            _context.Users.Add(newOwner);
            _context.SaveChanges();

            return Ok(new { message = "The new owner account has been successfully created!", userId = newOwner.UserId });
        }

        public class AddArenaRequest
        {
            public int OwnerId { get; set; }
            public string Name { get; set; } = string.Empty;
            public string City { get; set; } = string.Empty;
            public string Location { get; set; } = string.Empty;
        }

        [HttpPost("add-arena")]
        public IActionResult AddArena([FromBody] AddArenaRequest request)
        {
            var owner = _context.Users.FirstOrDefault(u => u.UserId == request.OwnerId && u.Role.RoleName == "Owner");
            if (owner == null)
            {
                return BadRequest(new { message = "Invalid ID! Either this user does not exist, or they are not the owner." });                
            }

            var newArena = new Arena
            {
                OwnerId = request.OwnerId,
                Name = request.Name,
                City = request.City,
                Location = request.Location,
                IsActive = true
            };

            _context.Arenas.Add(newArena);
            _context.SaveChanges();

            return Ok(new { message = "The new arena has been successfully created and assigned to the owner!", arenaId = newArena.ArenaId });
        }

        [HttpGet("all-owners")]
        public IActionResult GetAllArenas()
        {
            var arenas = _context.Arenas.Include(a => a.Owner).Select(a => new
            {
                a.ArenaId,
                ArenaName = a.Name,
                a.City,
                a.Location,
                Status = a.IsActive == true ? "Active" : "Blocked",
                OwnerName = a.Owner.FullName,
                OwnerPhone = a.Owner.Phone
            }).ToList();

            return Ok(arenas);
        }

        [HttpPut("toggle-arena-status/{arenaId}")]
        public IActionResult ToggleArenaStatus(int arenaId)
        {
            var arena = _context.Arenas.FirstOrDefault(a => a.ArenaId == arenaId);
            if (arena == null)
            {
                return NotFound(new { message = "No entry for this Id was found in the database." });
            }

            arena.IsActive = !arena.IsActive;
            _context.SaveChanges();

            string currentStatus = arena.IsActive == true ? "Active (Unblocked)" : "Deactivated (Blocked)";
            return Ok(new { message = "Status Update: This arena has now become {currentStatus}." });
        }
    }

    public class CreateOwnerRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
    }

    public class AddArenaRequest
    {
        public int OwnerId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
    }
}
