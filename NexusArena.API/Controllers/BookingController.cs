using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusArena.API.Models;
using Razorpay.Api; // 🌟 RAZORPAY NAMESPACE (Package install karne ke baad error chala jayega)
using System.Collections.Generic;

namespace NexusArena.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "User")]
    public class BookingController : ControllerBase
    {
        private readonly NexusArenaDbContext _context;

        public BookingController(NexusArenaDbContext context)
        {
            _context = context;
        }

        // 1. GET AVAILABLE SLOTS (As it is, perfectly working)
        [HttpGet("available-slots")]
        public async Task<IActionResult> GetAvailableSlots(int arenaId, string date)
        {
            try
            {
                if (!DateOnly.TryParse(date, out DateOnly playDate))
                    return BadRequest(new { message = "Invalid date format." });

                var resource = await _context.Resources.FirstOrDefaultAsync(r => r.ArenaId == arenaId);
                if (resource == null) return NotFound(new { message = "Turf resource not found." });

                var allSlots = await _context.TimeSlots
                    .Where(ts => ts.ResourceId == resource.ResourceId)
                    .ToListAsync();

                var bookedSlotIds = await _context.Bookings
                    .Where(b => b.ResourceId == resource.ResourceId && b.BookingDate == playDate && b.Status != "Cancelled")
                    .Select(b => b.SlotId)
                    .ToListAsync();

                var availabilityList = allSlots.Select(slot => new SlotAvailabilityDto
                {
                    SlotId = slot.SlotId,
                    StartTime = slot.StartTime.ToString("hh:mm tt"),
                    EndTime = slot.EndTime.ToString("hh:mm tt"),
                    Price = slot.BasePrice,
                    IsAvailable = !bookedSlotIds.Contains(slot.SlotId)
                }).ToList();

                return Ok(new { message = "Slots fetched", data = availabilityList });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // 2. CREATE BOOKING WITH RAZORPAY LOGIC 🌟
        [HttpPost("create")]
        public async Task<IActionResult> CreateBooking([FromBody] CreateBookingRequest request)
        {
            try
            {
                var userIdString = User.FindFirst("UserId")?.Value ?? User.FindFirst("id")?.Value;
                if (string.IsNullOrEmpty(userIdString)) return Unauthorized();

                int userId = int.Parse(userIdString);

                if (!DateOnly.TryParse(request.PlayDate, out DateOnly playDate))
                    return BadRequest(new { message = "Invalid date format." });

                var resource = await _context.Resources.FirstOrDefaultAsync(r => r.ArenaId == request.ArenaId);
                if (resource == null) return NotFound(new { message = "Turf not found." });

                // Check double booking
                var isAlreadyBooked = await _context.Bookings
                    .AnyAsync(b => b.ResourceId == resource.ResourceId && b.SlotId == request.SlotId && b.BookingDate == playDate && b.Status != "Cancelled");

                if (isAlreadyBooked) return BadRequest(new { message = "Slot already booked." });

                // 🌟 PRICE CALCULATION
                var slotInfo = await _context.TimeSlots.FindAsync(request.SlotId);
                if (slotInfo == null) return NotFound(new { message = "Slot details not found." });

                decimal amountToPay = 0;
                string paymentStatus = "Pending";

                if (request.PaymentMode == "Full")
                {
                    amountToPay = slotInfo.BasePrice;
                }
                else if (request.PaymentMode == "Advance50")
                {
                    amountToPay = slotInfo.BasePrice / 2;
                }

                // 🌟 SAVE BOOKING IN DATABASE (Initial Pending Status)
                var newBooking = new Booking
                {
                    UserId = userId,
                    ResourceId = resource.ResourceId,
                    SlotId = request.SlotId,
                    BookingDate = playDate,
                    Status = "Confirmed", // Booking ground par confirm ho gayi hai
                    PaymentMode = request.PaymentMode,
                    PaymentStatus = paymentStatus, // Par paisa aana baaki hai
                    AmountPaid = 0
                };

                _context.Bookings.Add(newBooking);
                await _context.SaveChangesAsync();

                // 🌟 RAZORPAY ORDER CREATION
                if (amountToPay > 0)
                {
                    // NOTE: Apne Razorpay account ki asli Test Keys yahan daalni hain
                    string key = "rzp_test_YOUR_KEY_HERE";
                    string secret = "YOUR_SECRET_HERE";

                    try
                    {
                        RazorpayClient client = new RazorpayClient(key, secret);

                        Dictionary<string, object> options = new Dictionary<string, object>();
                        options.Add("amount", amountToPay * 100); // Amount paise me hota hai isliye * 100
                        options.Add("currency", "INR");
                        options.Add("receipt", "rcpt_" + newBooking.BookingId);

                        Order order = client.Order.Create(options);
                        string razorpayOrderId = order["id"].ToString();

                        return Ok(new
                        {
                            message = "Razorpay order created",
                            bookingId = newBooking.BookingId,
                            requiresPayment = true,
                            razorpayOrderId = razorpayOrderId,
                            amount = amountToPay
                        });
                    }
                    catch (Exception rzpEx)
                    {
                        // Agar Razorpay order fail ho jaye
                        return StatusCode(500, new { message = "Payment Gateway Error: " + rzpEx.Message });
                    }
                }

                // 🌟 Agar PayAtTurf chuna hai (AmountToPay == 0)
                return Ok(new
                {
                    message = "Booking successful!",
                    bookingId = newBooking.BookingId,
                    requiresPayment = false
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}