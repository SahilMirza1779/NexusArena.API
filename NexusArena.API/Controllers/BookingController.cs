using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusArena.API.Models;
using Razorpay.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Mail;

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
        // 1. GET AVAILABLE TIME SLOTS 
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
                    .Where(b => b.ResourceId == resource.ResourceId && b.BookingDate == playDate && b.Status != "Cancelled" && b.Status != "Payment Failed")
                    .Select(b => b.SlotId)
                    .ToListAsync();

                var result = allSlots.Select(slot => new
                {
                    slotId = slot.SlotId,
                    timeDisplay = $"{slot.StartTime:hh\\:mm tt} - {slot.EndTime:hh\\:mm tt}",
                    price = slot.BasePrice,
                    isPremium = slot.IsPremium,
                    isBooked = bookedSlotIds.Contains(slot.SlotId)
                }).ToList();

                return Ok(new { data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ==========================================
        // 2. CREATE INITIAL BOOKING 
        // ==========================================
        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> CreateBooking([FromBody] CreateBookingRequest request)
        {
            try
            {
                var userIdString = User.Claims.FirstOrDefault(c => c.Type == "UserId" || c.Type == "id")?.Value;
                if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
                    return Unauthorized(new { message = "Invalid Token." });

                if (!DateOnly.TryParse(request.PlayDate, out DateOnly playDate))
                    return BadRequest(new { message = "Invalid date format." });

                var resource = await _context.Resources.FirstOrDefaultAsync(r => r.ArenaId == request.ArenaId);
                if (resource == null) return NotFound(new { message = "Turf not found." });

                var isAlreadyBooked = await _context.Bookings
                    .AnyAsync(b => b.ResourceId == resource.ResourceId && b.SlotId == request.SlotId && b.BookingDate == playDate && b.Status != "Cancelled" && b.Status != "Payment Failed");

                if (isAlreadyBooked) return BadRequest(new { message = "Slot already booked." });

                var slotInfo = await _context.TimeSlots.FindAsync(request.SlotId);
                if (slotInfo == null) return NotFound(new { message = "Slot details not found." });

                decimal amountToPay = 0;
                if (request.PaymentMode == "Full") amountToPay = slotInfo.BasePrice;
                else if (request.PaymentMode == "Advance50") amountToPay = slotInfo.BasePrice / 2;

                var newBooking = new Booking
                {
                    UserId = userId,
                    ResourceId = resource.ResourceId,
                    SlotId = request.SlotId,
                    BookingDate = playDate,
                    Status = "Pending Payment",
                    PaymentMode = request.PaymentMode,
                    PaymentStatus = "Pending",
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
                        newBooking.Status = "Payment Failed";
                        await _context.SaveChangesAsync();
                        return StatusCode(500, new { message = "Payment Gateway Error: " + rzpEx.Message });
                    }
                }

                newBooking.Status = "Confirmed";
                await _context.SaveChangesAsync();
                return Ok(new { message = "Booking successful!", bookingId = newBooking.BookingId, requiresPayment = false });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ==========================================
        // 3. SECURE PAYMENT VERIFICATION & EMAIL TICKET 
        // ==========================================
        [Authorize]
        [HttpPost("verify")]
        public async Task<IActionResult> VerifyPayment([FromBody] PaymentVerificationDto request)
        {
            try
            {
                string secret = "R4wy1mnJL59R0z76VKetdkGM";
                string payload = $"{request.RazorpayOrderId}|{request.RazorpayPaymentId}";
                string generatedSignature = "";

                using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret)))
                {
                    var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
                    generatedSignature = BitConverter.ToString(hash).Replace("-", "").ToLower();
                }

                if (generatedSignature != request.RazorpaySignature?.ToLower())
                {
                    return BadRequest(new { message = "Payment Verification Failed: Invalid Signature!" });
                }

                var booking = await _context.Bookings.FindAsync(request.BookingId);
                if (booking == null) return NotFound(new { message = "Booking not found" });

                if (booking.PaymentStatus == "Paid" && booking.Status == "Confirmed")
                {
                    return Ok(new { message = "Payment already verified and process completed." });
                }

                booking.PaymentStatus = "Paid";
                booking.Status = "Confirmed";
                booking.TransactionId = request.RazorpayPaymentId;

                var slot = await _context.TimeSlots.FindAsync(booking.SlotId);
                decimal basePrice = slot?.BasePrice ?? 0;

                if (booking.PaymentMode == "Full") booking.AmountPaid = basePrice;
                else if (booking.PaymentMode == "Advance50") booking.AmountPaid = basePrice / 2;

                await _context.SaveChangesAsync();

                // ========================================================
                // DIGITAL TICKET EMAIL LOGIC
                // ========================================================
                try
                {
                    var user = await _context.Users.FindAsync(booking.UserId);
                    if (user != null && !string.IsNullOrEmpty(user.Email))
                    {
                        string fromEmail = "sahilmirza01779@gmail.com";
                        string appPassword = "xumb xpgu rrbd aimt";

                        var smtpClient = new SmtpClient("smtp.gmail.com")
                        {
                            Port = 587,
                            Credentials = new NetworkCredential(fromEmail, appPassword),
                            EnableSsl = true,
                        };

                        var mailMessage = new MailMessage
                        {
                            From = new MailAddress(fromEmail, "Nexus Arena Premium"),
                            Subject = "🎟️ Your Turf Ticket is Confirmed! - Nexus Arena",
                            Body = $@"
                                <div style='background-color:#0f0f0f; color:#fff; padding:20px; font-family:Arial, sans-serif; border-radius:10px; border:1px solid #333;'>
                                    <h2 style='color:#00ff66;'>Booking Confirmed! ⚽</h2>
                                    <p style='color:#aaa;'>Hi {user.FullName}, your turf slot has been successfully locked.</p>
                                    <div style='background-color:#1a1a1a; padding:15px; border-left:4px solid #00ff66; margin:20px 0;'>
                                        <strong>Booking ID:</strong> #{booking.BookingId}<br/>
                                        <strong>Date:</strong> {booking.BookingDate.ToString("dd MMM yyyy")}<br/>
                                        <strong>Amount Paid:</strong> Rs. {booking.AmountPaid}<br/>
                                        <strong>Payment Mode:</strong> {booking.PaymentMode}
                                    </div>
                                    <p style='color:#888; font-size:12px;'>Please show this email at the arena counter.</p>
                                </div>",
                            IsBodyHtml = true,
                        };
                        mailMessage.To.Add(user.Email);
                        await smtpClient.SendMailAsync(mailMessage);
                    }
                }
                catch (Exception mailEx)
                {
                    Console.WriteLine("Email Send Failed: " + mailEx.Message);
                }
                // ========================================================

                return Ok(new { message = "Payment verified successfully and ticket emailed." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Verification Error: " + ex.Message });
            }
        }
    }

    public class CreateBookingRequest
    {
        public int ArenaId { get; set; }
        public int SlotId { get; set; }
        public string PlayDate { get; set; } = string.Empty;
        public string PaymentMode { get; set; } = string.Empty;
    }

    public class PaymentVerificationDto
    {
        public int BookingId { get; set; }
        public string RazorpayPaymentId { get; set; } = string.Empty;
        public string RazorpayOrderId { get; set; } = string.Empty;
        public string RazorpaySignature { get; set; } = string.Empty;
    }
}