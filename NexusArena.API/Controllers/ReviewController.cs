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

        // 🌟 ENTERPRISE FIX: Sirf user ki past Confirmed/Completed bookings bhejo
        [HttpGet("eligible-bookings")]
        public async Task<IActionResult> GetEligibleBookings()
        {
            try
            {
                var userIdString = User.Claims.FirstOrDefault(c => c.Type == "UserId" || c.Type == "id")?.Value;
                if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId)) return Unauthorized();

                var pastBookings = await _context.Bookings
                    .Where(b => b.UserId == userId && b.Status == "Confirmed") // Status match karna apne hisaab se
                    .Select(b => new
                    {
                        bookingId = b.BookingId,
                        arenaId = b.ResourceId,
                        arenaName = _context.Arenas.FirstOrDefault(a => a.ArenaId == b.ResourceId).Name,
                        playDate = b.BookingDate
                    })
                    .OrderByDescending(b => b.playDate)
                    .ToListAsync();

                return Ok(new { data = pastBookings });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching bookings: " + ex.Message });
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
                    BookingId = request.BookingId, // 🌟 Save Booking ID
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
                        bookingId = r.BookingId, // 🌟 Return Booking ID
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
        public int BookingId { get; set; } // 🌟 Added
        public int Rating { get; set; }
        public string? Comment { get; set; }
    }
}