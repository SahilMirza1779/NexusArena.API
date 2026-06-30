using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusArena.API.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace NexusArena.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReviewController : ControllerBase
    {
        private readonly NexusArenaDbContext _context;

        public ReviewController(NexusArenaDbContext context)
        {
            _context = context;
        }

        // 🌟 NAYA METHOD: Dropdown bharne ke liye saare Turfs bhejo
        [HttpGet("arenas")]
        public async Task<IActionResult> GetArenasForDropdown()
        {
            try
            {
                var arenas = await _context.Arenas
                    .Where(a => a.IsActive == true)
                    .Select(a => new
                    {
                        arenaId = a.ArenaId,
                        name = a.Name
                    })
                    .ToListAsync();

                return Ok(new { data = arenas });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching arenas: " + ex.Message });
            }
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddReview([FromBody] CreateReviewDto request)
        {
            try
            {
                var userIdString = User.Claims.FirstOrDefault(c => c.Type == "UserId" || c.Type == "id")?.Value;
                if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId)) return Unauthorized();

                if (request.Rating < 1 || request.Rating > 5)
                    return BadRequest(new { message = "Rating 1 se 5 ke beech honi chahiye." });

                var newReview = new Review
                {
                    UserId = userId,
                    ArenaId = request.ArenaId,
                    Rating = request.Rating,
                    Comment = request.Comment,
                    CreatedAt = DateTime.Now
                };

                _context.Reviews.Add(newReview);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Aapka review successfully save ho gaya!" });
            }
            catch (Exception ex) { return StatusCode(500, new { message = "Internal server error", error = ex.Message }); }
        }

        [HttpGet("my-reviews")]
        public async Task<IActionResult> GetMyReviews()
        {
            try
            {
                var userIdString = User.Claims.FirstOrDefault(c => c.Type == "UserId" || c.Type == "id")?.Value;
                if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId)) return Unauthorized();

                var myReviews = await _context.Reviews
                    .Where(r => r.UserId == userId)
                    .Include(r => r.Arena)
                    .Select(r => new
                    {
                        reviewId = r.ReviewId,
                        arenaId = r.ArenaId,
                        arenaName = r.Arena.Name,
                        rating = r.Rating,
                        comment = r.Comment,
                        date = r.CreatedAt ?? DateTime.Now
                    })
                    .OrderByDescending(r => r.date)
                    .ToListAsync();

                return Ok(new { message = "Reviews fetched successfully", data = myReviews });
            }
            catch (Exception ex) { return StatusCode(500, new { message = "Internal server error", error = ex.Message }); }
        }

        [HttpPut("update/{reviewId}")]
        public async Task<IActionResult> UpdateReview(int reviewId, [FromBody] CreateReviewDto request)
        {
            try
            {
                var userIdString = User.Claims.FirstOrDefault(c => c.Type == "UserId" || c.Type == "id")?.Value;
                if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId)) return Unauthorized();

                var review = await _context.Reviews.FirstOrDefaultAsync(r => r.ReviewId == reviewId && r.UserId == userId);
                if (review == null) return NotFound(new { message = "Review nahi mila." });

                review.Rating = request.Rating;
                review.Comment = request.Comment;

                await _context.SaveChangesAsync();
                return Ok(new { message = "Review successfully update ho gaya!" });
            }
            catch (Exception ex) { return StatusCode(500, new { message = "Internal server error", error = ex.Message }); }
        }

        [HttpDelete("delete/{reviewId}")]
        public async Task<IActionResult> DeleteReview(int reviewId)
        {
            try
            {
                var userIdString = User.Claims.FirstOrDefault(c => c.Type == "UserId" || c.Type == "id")?.Value;
                if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId)) return Unauthorized();

                var review = await _context.Reviews.FirstOrDefaultAsync(r => r.ReviewId == reviewId && r.UserId == userId);
                if (review == null) return NotFound(new { message = "Review nahi mila." });

                _context.Reviews.Remove(review);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Review delete ho gaya!" });
            }
            catch (Exception ex) { return StatusCode(500, new { message = "Internal server error", error = ex.Message }); }
        }
    }

    public class CreateReviewDto
    {
        public int ArenaId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
    }
}