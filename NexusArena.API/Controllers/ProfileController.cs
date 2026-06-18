using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusArena.API.Models;

[Authorize(Roles = "User")]
[Route("api/[controller]")]
[ApiController]
public class ProfileController : ControllerBase
{
    private readonly NexusArenaDbContext _context;
    public ProfileController(NexusArenaDbContext context) { _context = context; }

    [HttpGet("me")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = int.Parse(User.FindFirst("UserId")?.Value);
        var user = await _context.Users.FindAsync(userId);
        return Ok(user);
    }

    [HttpPut("update")]
    public async Task<IActionResult> UpdateProfile([FromBody] UserUpdateDto dto)
    {
        var userId = int.Parse(User.FindFirst("UserId")?.Value);
        var user = await _context.Users.FindAsync(userId);
        user.FullName = dto.FullName;
        user.PhoneNumber = dto.PhoneNumber;
        await _context.SaveChangesAsync();
        return Ok(new { message = "Profile Updated!" });
    }
}