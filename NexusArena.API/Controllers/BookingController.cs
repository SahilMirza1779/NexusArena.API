using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusArena.API.Models;
using Razorpay.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace NexusArena.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly NexusArenaDbContext _context;

        public BookingController(NexusArenaDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. GET AVAILABLE TIME SLOTS (Fully Open)
        // ==========================================
        [AllowAnonymous]
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

        // ==========================================
        // 2. CREATE INITIAL BOOKING (Secured)
        // ==========================================
        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> CreateBooking([FromBody] CreateBookingRequest request)
        {
            try
            {
                // 🌟 THE FIX: Sirf "UserId" ya "id" uthayega, aur check karega ki wo Number hai ya nahi! Email ko reject kar dega.
                var userIdString = User.Claims.FirstOrDefault(c => c.Type == "UserId" || c.Type == "id")?.Value;

                if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
                {
                    return Unauthorized(new { message = "Invalid Token: User ID must be a number." });
                }

                if (!DateOnly.TryParse(request.PlayDate, out DateOnly playDate))
                    return BadRequest(new { message = "Invalid date format." });

                var resource = await _context.Resources.FirstOrDefaultAsync(r => r.ArenaId == request.ArenaId);
                if (resource == null) return NotFound(new { message = "Turf not found." });

                var isAlreadyBooked = await _context.Bookings
                    .AnyAsync(b => b.ResourceId == resource.ResourceId && b.SlotId == request.SlotId && b.BookingDate == playDate && b.Status != "Cancelled");

                if (isAlreadyBooked) return BadRequest(new { message = "Slot already booked." });

                var slotInfo = await _context.TimeSlots.FindAsync(request.SlotId);
                if (slotInfo == null) return NotFound(new { message = "Slot details not found." });

                decimal amountToPay = 0;
                string paymentStatus = "Pending";

                if (request.PaymentMode == "Full") amountToPay = slotInfo.BasePrice;
                else if (request.PaymentMode == "Advance50") amountToPay = slotInfo.BasePrice / 2;

                var newBooking = new Booking
                {
                    UserId = userId,
                    ResourceId = resource.ResourceId,
                    SlotId = request.SlotId,
                    BookingDate = playDate,
                    Status = "Confirmed",
                    PaymentMode = request.PaymentMode,
                    PaymentStatus = paymentStatus,
                    AmountPaid = 0
                };

                _context.Bookings.Add(newBooking);
                await _context.SaveChangesAsync();

                if (amountToPay > 0)
                {
                    string key = "rzp_test_Sx2ANZO6KtKqPv";
                    string secret = "R4wy1mnJL59R0z76VKetdkGM";

                    try
                    {
                        RazorpayClient client = new RazorpayClient(key, secret);
                        Dictionary<string, object> options = new Dictionary<string, object>
                        {
                            { "amount", amountToPay * 100 },
                            { "currency", "INR" },
                            { "receipt", "rcpt_" + newBooking.BookingId }
                        };

                        Order order = client.Order.Create(options);
                        return Ok(new
                        {
                            message = "Razorpay order created",
                            bookingId = newBooking.BookingId,
                            requiresPayment = true,
                            razorpayOrderId = order["id"].ToString(),
                            amount = amountToPay
                        });
                    }
                    catch (Exception rzpEx)
                    {
                        return StatusCode(500, new { message = "Payment Gateway Error: " + rzpEx.Message });
                    }
                }

                return Ok(new { message = "Booking successful!", bookingId = newBooking.BookingId, requiresPayment = false });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ==========================================
        // 3. SECURE PAYMENT VERIFICATION (Secured)
        // ==========================================
        [Authorize]
        [HttpPost("verify")]
        public async Task<IActionResult> VerifyPayment([FromBody] PaymentVerificationDto request)
        {
            try
            {
                string secret = "R4wy1mnJL59R0z76VKetdkGM";
                var attributes = new Dictionary<string, string>
                {
                    { "razorpay_payment_id", request.RazorpayPaymentId },
                    { "razorpay_order_id", request.RazorpayOrderId },
                    { "razorpay_signature", request.RazorpaySignature }
                };

                Utils.verifyPaymentSignature(attributes);

                var booking = await _context.Bookings.FindAsync(request.BookingId);
                if (booking == null) return NotFound("Booking not found");

                booking.PaymentStatus = "Paid";
                booking.TransactionId = request.RazorpayPaymentId;

                var slot = await _context.TimeSlots.FindAsync(booking.SlotId);
                if (booking.PaymentMode == "Full") booking.AmountPaid = slot?.BasePrice ?? 0;
                else if (booking.PaymentMode == "Advance50") booking.AmountPaid = (slot?.BasePrice ?? 0) / 2;

                await _context.SaveChangesAsync();

                return Ok(new { message = "Payment verified successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Payment Verification Failed: " + ex.Message });
            }
        }
    }
}