using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusArena.API.Models;

namespace NexusArena.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ManageRevenueController : ControllerBase
    {
        private readonly NexusArenaDbContext _context;

        public ManageRevenueController(NexusArenaDbContext context)
        {
            _context = context;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAllTransactions()
        {
            var transactions = await _context.Payments
                .Include(p => p.Booking)
                .ThenInclude(b => b.User)
                .Select(p => new
                {
                    Id = p.PaymentId,
                    UserName = p.Booking.User.FullName ?? "Unknown User",
                    Amount = p.TotalAmount,
                    Date = p.Booking.BookingDate,
                    Status = p.Booking.Status,
                    Method = p.PaymentMethod ?? "Online",
                })
                .OrderByDescending(p => p.Id)
                .ToListAsync();
            return Ok(transactions);
        }
    }
}
