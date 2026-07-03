using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NexusArena.API.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;

namespace NexusArena.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly NexusArenaDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(NexusArenaDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpGet("debug-roles")]
        [AllowAnonymous]
        public IActionResult DebugRoles()
        {
            var roles = _context.Roles.ToList();
            var users = _context.Users.Include(u => u.Role).ToList();

            return Ok(new
            {
                roles = roles.Select(r => new { r.RoleId, r.RoleName }),
                users = users.Select(u => new { u.UserId, u.Email, u.FullName, u.IsActive, Role = u.Role?.RoleName, PasswordHashLength = u.PasswordHash.Length })
            });
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _context.Users
                .AsNoTracking()
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null || user.IsActive == false)
            {
                return Unauthorized(new { message = "Invalid Email or Password" });
            }

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return Unauthorized(new { message = "Invalid Email or Password" });
            }

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim("UserId", user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, user.Role.RoleName)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
             );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            Response.Cookies.Append("JWToken", tokenString, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddHours(2)
            });

            return Ok(new
            {
                token = tokenString,
                role = user.Role.RoleName,
                message = "Login Successful"
            });
        }

        [HttpPost("Register")]
        [AllowAnonymous]
        public IActionResult Register([FromBody] RegisterRequestDto request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new { message = "Email and Password are required." });
            }

            if (_context.Users.Any(u => u.Email == request.Email))
            {
                return BadRequest(new { message = "Email already exists." });
            }

            var role = _context.Roles.FirstOrDefault(r => r.RoleName == request.RoleName);
            if (role == null)
            {
                return BadRequest(new { message = $"Role '{request.RoleName}' not found in database. Available roles: Customer, Owner, Receptionist, SuperAdmin" });
            }

            try
            {
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

                var newUser = new User
                {
                    FullName = request.FullName,
                    Email = request.Email,
                    Phone = request.Phone,
                    PasswordHash = hashedPassword,
                    RoleId = role.RoleId,
                    IsActive = true
                };

                _context.Users.Add(newUser);
                _context.SaveChanges();

                return Ok(new { message = "Registration successful", userId = newUser.UserId, email = newUser.Email });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Registration failed", error = ex.Message });
            }
        }

        [HttpPost("VerifyEmail")]
        [AllowAnonymous]
        public IActionResult VerifyEmail([FromBody] ForgotPasswordRequestDto request)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == request.Email);
            if (user == null)
            {
                return NotFound(new { message = "Email not found" });
            }
            return Ok();
        }

        [HttpPost("ResetUserPassword")]
        [AllowAnonymous]
        public IActionResult ResetUserPassword([FromBody] ResetPasswordDto request)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == request.Email);
            if (user == null) return NotFound();

            user.PasswordHash = request.NewPassword;
            _context.SaveChanges();

            return Ok(new { message = "Password updated successfully" });
        }

        public class ForgotPasswordRequestDto { public string Email { get; set; } }
        public class ResetPasswordDto { public string Email { get; set; } public string NewPassword { get; set; } }

        public class RegisterRequestDto
        {
            public string FullName { get; set; }
            public string Email { get; set; }
            public string Phone { get; set; }
            public string Password { get; set; }
            public string RoleName { get; set; }
        }
    }
}