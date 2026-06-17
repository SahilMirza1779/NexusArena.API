using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusArena.API.Models;

namespace NexusArena.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "User")] // Sirf login kiya hua player hi review de sakta hai
    public class ReviewController : ControllerBase
    {
        private readonly NexusArenaDbContext _context;

        public ReviewController(NexusArenaDbContext context)
        {
            _context = context;
        }

        // 1. API: Naya Review Save Karna
        [HttpPost("add")]
        public async Task<IActionResult> AddReview([FromBody] CreateReviewDto request)
        {
            try
            {
                // Token se User ID nikalna
                var userIdString = User.FindFirst("UserId")?.Value;
                if (string.IsNullOrEmpty(userIdString)) return Unauthorized();
                int userId = int.Parse(userIdString);

                // Rating check karna (1 se 5 ke beech honi chahiye)
                if (request.Rating < 1 || request.Rating > 5)
                {
                    return BadRequest(new { message = "Rating 1 se 5 ke beech honi chahiye." });
                }

                // Database me review save karna
                var newReview = new Review
                {
                    UserId = userId,
                    ArenaId = request.ArenaId,
                    Rating = request.Rating,
                    Comment = request.Comment,
                    CreatedAt = DateTime.Now // Date automatic set hogi
                };

                _context.Reviews.Add(newReview);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Aapka review successfully save ho gaya!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // 2. API: Player ke saare purane reviews fetch karna
        [HttpGet("my-reviews")]
        public async Task<IActionResult> GetMyReviews()
        {
            try
            {
                var userIdString = User.FindFirst("UserId")?.Value;
                if (string.IsNullOrEmpty(userIdString)) return Unauthorized();
                int userId = int.Parse(userIdString);

                // User ke saare reviews aur Turf (Arena) ka naam nikalna
                var myReviews = await _context.Reviews
                    .Where(r => r.UserId == userId)
                    .Include(r => r.Arena)
                    .Select(r => new
                    {
                        ReviewId = r.ReviewId,
                        ArenaId = r.ArenaId,
                        ArenaName = r.Arena.Name, // Arena table se naam utha rahe hain
                        Rating = r.Rating,
                        Comment = r.Comment,
                        Date = r.CreatedAt
                    })
                    .OrderByDescending(r => r.Date)
                    .ToListAsync();

                return Ok(new { message = "Reviews fetched successfully", data = myReviews });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }
    }

    // DTO: Frontend se data lene ke liye
    public class CreateReviewDto
    {
        public int ArenaId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
    }
}