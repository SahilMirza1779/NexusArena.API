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

        [AllowAnonymous]
        [HttpGet("booked-times")]
        public async Task<IActionResult> GetBookedTimes(int resourceId, string date)
        {
            try
            {
                if (!DateOnly.TryParseExact(date, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out DateOnly playDate))
                    return BadRequest(new { message = "Invalid date format." });

                var bookedRanges = await _context.Bookings
                    .Where(b => b.ResourceId == resourceId &&
                                b.BookingDate == playDate &&
                                b.Status != "Cancelled" &&
                                b.Status != "Payment Failed")
                    .Select(b => new {
                        start = b.StartTime,
                        end = b.EndTime
                    })
                    .ToListAsync();

                return Ok(new { success = true, data = bookedRanges });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> CreateBooking([FromBody] CreateBookingRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.BookingMode))
                {
                    request.BookingMode = "Hourly";
                }

                var userIdString = User.Claims.FirstOrDefault(c => c.Type == "UserId" || c.Type == "id")?.Value;
                if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
                    return Unauthorized(new { message = "Invalid Token." });

                if (!DateOnly.TryParseExact(request.PlayDate, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out DateOnly playDate))
                    return BadRequest(new { message = "Invalid play date format." });

                var resource = await _context.Resources
                    .Include(r => r.Arena)
                    .FirstOrDefaultAsync(r => r.ResourceId == request.ResourceId || r.ArenaId == request.ResourceId);

                if (resource == null) return NotFound(new { message = $"Turf with ID {request.ResourceId} not found in Database." });

                TimeOnly requestedStart = default;
                TimeOnly requestedEnd = default;

                if (request.BookingMode == "Hourly")
                {
                    if (!TimeOnly.TryParse(request.StartTime, out requestedStart))
                        return BadRequest(new { message = $"Invalid Start Time format: {request.StartTime}" });

                    if (!TimeOnly.TryParse(request.EndTime, out requestedEnd))
                        return BadRequest(new { message = $"Invalid End Time format: {request.EndTime}" });
                }

                if (request.BookingMode == "Hourly")
                {
                    bool isClashing = await _context.Bookings.AnyAsync(b =>
                        b.ResourceId == resource.ResourceId &&
                        b.BookingDate == playDate &&
                        b.Status != "Cancelled" && b.Status != "Payment Failed" &&
                        b.BookingMode == "Hourly" &&
                        b.StartTime != null && b.EndTime != null &&
                        b.StartTime < requestedEnd && b.EndTime > requestedStart);

                    if (isClashing)
                        return BadRequest(new { message = "Selected time overlaps with an existing booking." });
                }

                decimal totalAmount = 0;
                if (request.BookingMode == "Hourly")
                {
                    int startHour = requestedStart.Hour;
                    int endHour = requestedEnd.Hour;
                    if (endHour <= startHour) endHour += 24;

                    for (int h = startHour; h < endHour; h++)
                    {
                        int currentHour = h % 24;
                        if (currentHour >= 6 && currentHour < 17)
                            totalAmount += resource.Arena.HourlyRegularPrice;
                        else
                            totalAmount += resource.Arena.HourlyPeakPrice;
                    }
                }
                else if (request.BookingMode == "Tournament")
                {
                    if (request.TournamentPackage == "HalfDayMorning") totalAmount = resource.Arena.HalfDayMorningPrice;
                    else if (request.TournamentPackage == "HalfDayEvening") totalAmount = resource.Arena.HalfDayEveningPrice;
                    else if (request.TournamentPackage == "FullDay") totalAmount = resource.Arena.FullDayPrice;
                }

                if (totalAmount <= 0) totalAmount = 800;

                decimal amountToPay = request.PaymentMode == "Advance50" ? (totalAmount / 2) : totalAmount;

                var newBooking = new Booking
                {
                    UserId = userId,
                    ResourceId = resource.ResourceId,
                    BookingDate = playDate,
                    StartTime = request.BookingMode == "Hourly" ? requestedStart : null,
                    EndTime = request.BookingMode == "Hourly" ? requestedEnd : null,
                    BookingMode = request.BookingMode,
                    TournamentPackage = request.TournamentPackage,
                    Status = "Pending Payment",
                    PaymentMode = request.PaymentMode,
                    PaymentStatus = "Pending",
                    TotalAmount = totalAmount,
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
                        int amountInPaisa = Convert.ToInt32(amountToPay * 100);

                        Dictionary<string, object> options = new Dictionary<string, object>
                        {
                            { "amount", amountInPaisa },
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
                            amount = amountToPay,
                            totalBill = totalAmount
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
            catch (DbUpdateException dbEx)
            {
                Exception inner = dbEx;
                while (inner.InnerException != null)
                {
                    inner = inner.InnerException;
                }
                return StatusCode(500, new { message = "SQL DATABASE ERROR: " + inner.Message });
            }
            catch (Exception ex)
            {
                Exception inner = ex;
                while (inner.InnerException != null)
                {
                    inner = inner.InnerException;
                }
                return StatusCode(500, new { message = "SERVER CRASH ERROR: " + inner.Message });
            }
        }

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
                    return BadRequest(new { message = "Payment Verification Failed: Invalid Signature!" });

                var booking = await _context.Bookings.FindAsync(request.BookingId);
                if (booking == null) return NotFound(new { message = "Booking not found" });

                if (booking.PaymentStatus == "Paid" && booking.Status == "Confirmed")
                    return Ok(new { message = "Payment already verified." });

                booking.PaymentStatus = "Paid";
                booking.Status = "Confirmed";
                booking.TransactionId = request.RazorpayPaymentId;

                if (booking.PaymentMode == "Full") booking.AmountPaid = booking.TotalAmount;
                else if (booking.PaymentMode == "Advance50") booking.AmountPaid = booking.TotalAmount / 2;

                await _context.SaveChangesAsync();
                return Ok(new { message = "Payment verified successfully!" });
            }
            catch (Exception ex)
            {
                Exception inner = ex;
                while (inner.InnerException != null)
                {
                    inner = inner.InnerException;
                }
                return StatusCode(500, new { message = "Verification Error: " + inner.Message });
            }
        }
    }
}