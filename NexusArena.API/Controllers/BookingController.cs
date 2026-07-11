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

namespace NexusArena.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // 🌟 IDE0290 FIX: Primary Constructor use kiya (Modern C# Standard)
    public class BookingController(NexusArenaDbContext context) : ControllerBase
    {
        private readonly NexusArenaDbContext _context = context;
        private readonly IEmailService _emailService = emailService;

        public BookingController(NexusArenaDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // 1. GET BOOKED SLOTS
        // =========================================================
        [AllowAnonymous]
        [HttpGet("booked-times")]
        public async Task<IActionResult> GetBookedTimes(int resourceId, string date)
        {
            try
            {
                if (!DateOnly.TryParseExact(date, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out DateOnly playDate))
                    return BadRequest(new { message = "Invalid date format." });

                // Fetch minimal data to avoid EF Core translation errors
                var dbBookings = await _context.Bookings
                    .Where(b => b.ResourceId == resourceId &&
                                b.BookingDate == playDate &&
                                b.Status != "Cancelled" &&
                                b.Status != "Payment Failed")
                    .Select(b => new { b.BookingMode, b.TournamentPackage, b.StartTime, b.EndTime })
                    .ToListAsync();

                // Convert Tournament packages to actual times so UI can grey out boxes
                var normalizedRanges = dbBookings.Select(b => {
                    if (b.BookingMode == "Tournament")
                    {
                        if (b.TournamentPackage == "HalfDayMorning") return new { Start = "07:00", End = "14:00" };
                        if (b.TournamentPackage == "HalfDayEvening") return new { Start = "14:00", End = "21:00" };
                        return new { Start = "06:00", End = "24:00" }; // FullDay
                    }
                    return new
                    {
                        Start = b.StartTime?.ToString("HH:mm"),
                        End = b.EndTime?.ToString("HH:mm")
                    };
                }).ToList();

                return Ok(new { success = true, data = normalizedRanges });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // =========================================================
        // 2. CREATE SMART BOOKING (WITH DEEP SQL ERROR TRACKER)
        // =========================================================
        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> CreateBooking([FromBody] CreateBookingRequest request)
        {
            try
            {
                var userIdString = User.Claims.FirstOrDefault(c => c.Type == "UserId" || c.Type == "id")?.Value;
                if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
                    return Unauthorized(new { message = "Invalid Token." });

                if (!DateOnly.TryParseExact(request.PlayDate, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out DateOnly playDate))
                    return BadRequest(new { message = "Invalid play date format." });

                DateTime currentUtc = DateTime.UtcNow;
                TimeZoneInfo istZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                DateTime currentIst = TimeZoneInfo.ConvertTimeFromUtc(currentUtc, istZone);
                DateOnly todayIst = DateOnly.FromDateTime(currentIst);

                if (playDate < todayIst)
                    return BadRequest(new { message = "Booking failed: You cannot book slots for past dates." });

                var resource = await _context.Resources
                    .Include(r => r.Arena)
                    .FirstOrDefaultAsync(r => r.ResourceId == request.ResourceId || r.ArenaId == request.ResourceId);

                if (resource == null) return NotFound(new { message = $"Turf with ID {request.ResourceId} not found in Database." });

                // 🌟 FIX: Determine Exact Start/End for BOTH Hourly & Tournament
                TimeOnly requestedStart = default;
                TimeOnly requestedEnd = default;

                if (request.BookingMode == "Hourly")
                {
                    if (!TimeOnly.TryParse(request.StartTime, out requestedStart))
                        return BadRequest(new { message = $"Invalid Start Time format: {request.StartTime}" });

                    if (!TimeOnly.TryParse(request.EndTime, out requestedEnd))
                        return BadRequest(new { message = $"Invalid End Time format: {request.EndTime}" });

                    if (playDate == todayIst && requestedStart.Hour <= currentIst.Hour + 1)
                    {
                        return BadRequest(new { message = "Booking failed: A minimum 1-Hour advance notice is required for today's bookings. Please select a later time slot." });
                    }
                }
                else if (request.BookingMode == "Tournament")
                {
                    if (request.TournamentPackage == "HalfDayMorning") { requestedStart = new TimeOnly(7, 0); requestedEnd = new TimeOnly(14, 0); }
                    else if (request.TournamentPackage == "HalfDayEvening") { requestedStart = new TimeOnly(14, 0); requestedEnd = new TimeOnly(21, 0); }
                    else { requestedStart = new TimeOnly(6, 0); requestedEnd = new TimeOnly(23, 59, 59); } // Full Day
                }

                // 🌟 FIX: Universal Database Clash Check
                var existingBookings = await _context.Bookings
                    .Where(b => b.ResourceId == resource.ResourceId &&
                                b.BookingDate == playDate &&
                                b.Status != "Cancelled" && b.Status != "Payment Failed")
                    .ToListAsync();

                bool isClashing = false;
                foreach (var b in existingBookings)
                {
                    TimeOnly bStart = default;
                    TimeOnly bEnd = default;

                    if (b.BookingMode == "Tournament")
                    {
                        if (b.TournamentPackage == "HalfDayMorning") { bStart = new TimeOnly(7, 0); bEnd = new TimeOnly(14, 0); }
                        else if (b.TournamentPackage == "HalfDayEvening") { bStart = new TimeOnly(14, 0); bEnd = new TimeOnly(21, 0); }
                        else { bStart = new TimeOnly(6, 0); bEnd = new TimeOnly(23, 59, 59); }
                    }
                    else
                    {
                        if (!b.StartTime.HasValue || !b.EndTime.HasValue) continue;
                        bStart = b.StartTime.Value;
                        bEnd = b.EndTime.Value;
                    }

                    // Strict overlap check
                    if (bStart < requestedEnd && bEnd > requestedStart)
                    {
                        isClashing = true;
                        break;
                    }
                }

                if (isClashing)
                    return BadRequest(new { message = "Booking Clash: This slot overlaps with an existing Hourly booking or Tournament event." });

                decimal totalAmount = 0;
                if (request.BookingMode == "Hourly")
                {
                    int startHour = requestedStart.Hour;
                    int endHour = requestedEnd.Hour;
                    if (endHour <= startHour) endHour += 24;

                    for (int h = startHour; h < endHour; h++)
                    {
                        int currentHour = h % 24;
                        if (currentHour >= 6 && currentHour < 17) totalAmount += resource.Arena.HourlyRegularPrice;
                        else totalAmount += resource.Arena.HourlyPeakPrice;
                    }
                }
                else if (request.BookingMode == "Tournament")
                {
                    if (request.TournamentPackage == "HalfDayMorning") totalAmount = resource.Arena.HalfDayMorningPrice;
                    else if (request.TournamentPackage == "HalfDayEvening") totalAmount = resource.Arena.HalfDayEveningPrice;
                    else if (request.TournamentPackage == "FullDay") totalAmount = resource.Arena.FullDayPrice;
                }

                decimal amountToPay = request.PaymentMode == "Advance50" ? (totalAmount / 2) : totalAmount;

                Booking newBooking = new()
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

                // 🚨 DATABASE SAVE COMMAND (YAHI PAR CRASH HOTA THA)
                await _context.SaveChangesAsync();

                // ONLINE PAYMENT LOGIC
                if (amountToPay > 0)
                {
                    string key = "rzp_test_Sx2ANZO6KtKqPv";
                    string secret = "R4wy1mnJL59R0z76VKetdkGM";

                    try
                    {
                        RazorpayClient client = new(key, secret);
                        int amountInPaisa = Convert.ToInt32(amountToPay * 100);

                        Dictionary<string, object> options = new()
                        {
                            ["amount"] = amountInPaisa,
                            ["currency"] = "INR",
                            ["receipt"] = "rcpt_" + newBooking.BookingId
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

                return BadRequest(new { message = "Invalid Payment Mode or Amount." });
            }
            catch (DbUpdateException dbEx)
            {
                Exception inner = dbEx;
                while (inner.InnerException != null) inner = inner.InnerException;
                return StatusCode(500, new { message = "SQL DATABASE ERROR: " + inner.Message });
            }
            catch (Exception ex)
            {
                Exception inner = ex;
                while (inner.InnerException != null) inner = inner.InnerException;
                return StatusCode(500, new { message = "SERVER CRASH ERROR: " + inner.Message });
            }
        }

        // =========================================================
        // 3. SECURE PAYMENT VERIFICATION
        // =========================================================
        [Authorize]
        [HttpPost("verify")]
        public async Task<IActionResult> VerifyPayment([FromBody] PaymentVerificationDto request)
        {
            try
            {
                string secret = "R4wy1mnJL59R0z76VKetdkGM";
                string payload = $"{request.RazorpayOrderId}|{request.RazorpayPaymentId}";
                string generatedSignature;

                using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret)))
                {
                    var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
                    generatedSignature = Convert.ToHexStringLower(hash);
                }

                if (generatedSignature != request.RazorpaySignature?.ToLower())
                    return BadRequest(new { message = "Payment Verification Failed: Invalid Signature!" });

                var booking = await _context.Bookings
                    .Include(b => b.User)
                    .Include(b => b.Resource).ThenInclude(r => r.Arena)
                    .FirstOrDefaultAsync(b => b.BookingId == request.BookingId);

                if (booking == null) return NotFound(new { message = "Booking not found" });

                if (booking.PaymentStatus == "Paid" && booking.Status == "Confirmed")
                    return Ok(new { message = "Payment already verified." });

                booking.PaymentStatus = "Paid";
                booking.Status = "Confirmed";
                booking.TransactionId = request.RazorpayPaymentId;

                if (booking.PaymentMode == "Full") booking.AmountPaid = booking.TotalAmount;
                else if (booking.PaymentMode == "Advance50") booking.AmountPaid = booking.TotalAmount / 2;

                await _context.SaveChangesAsync();

                if (booking.User != null && !string.IsNullOrEmpty(booking.User.Email))
                {
                    string timeStr = booking.BookingMode == "Hourly" && booking.StartTime != null && booking.EndTime != null
                        ? $"{booking.StartTime.Value:hh\\:mm tt} - {booking.EndTime.Value:hh\\:mm tt}"
                        : booking.TournamentPackage ?? "Full Day";

                    string playerName = booking.User.Email.Split('@')[0];

                    _ = _emailService.SendBookingConfirmationAsync(
                        booking.User.Email,
                        playerName,
                        booking.Resource?.Arena?.Name ?? "Nexus Turf",
                        booking.BookingDate.ToString("dd MMM yyyy"),
                        timeStr,
                        booking.BookingId.ToString()
                    );
                }

                return Ok(new { message = "Payment verified successfully!" });
            }
            catch (Exception ex)
            {
                Exception inner = ex;
                while (inner.InnerException != null) inner = inner.InnerException;
                return StatusCode(500, new { message = "Verification Error: " + inner.Message });
            }
        }
    }
}