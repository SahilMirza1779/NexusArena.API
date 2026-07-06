using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace NexusArena.Web.Controllers
{
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public class BookingController : Controller
    {
        private readonly HttpClient _httpClient;
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public BookingController()
        {
            _httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:5092/") };
        }

        [HttpGet]
        public async Task<IActionResult> SelectSlot(int arenaId, string? date)
        {
            var token = Request.Cookies["JWToken"];
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Account");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            string selectedDate = string.IsNullOrEmpty(date) ? DateTime.Now.ToString("yyyy-MM-dd") : date;

            ViewBag.ArenaId = arenaId;
            ViewBag.SelectedDate = selectedDate;

            try
            {
                var response = await _httpClient.GetAsync($"api/Booking/booked-times?resourceId={arenaId}&date={selectedDate}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<BookedTimesResponse>(json, _jsonOptions);
                    ViewBag.BookedTimesJson = JsonSerializer.Serialize(result?.Data ?? new List<BookedTimeRange>());
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
            }

            return View();
        }

        [HttpPost]
        public IActionResult Review(int arenaId, string playDate, string startTime, string endTime, string bookingMode, string? tournamentPackage, decimal totalBill)
        {
            ViewBag.ArenaId = arenaId;
            ViewBag.Date = playDate;
            ViewBag.StartTime = startTime;
            ViewBag.EndTime = endTime;
            ViewBag.BookingMode = bookingMode;
            ViewBag.TournamentPackage = tournamentPackage;
            ViewBag.TotalBill = totalBill;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Confirm(int arenaId, string playDate, string startTime, string endTime, string bookingMode, string? tournamentPackage, string paymentMode, decimal totalBill)
        {
            var token = Request.Cookies["JWToken"];
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Account");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // 🌟 MASTER FIX: C# ab time ko bilkul nahi chhedega! 
            // JavaScript se jo ekdum perfect time aayega (jaise 23:59 ya actual Tournament hours), 
            // hum wahi direct API ko bhejenge taaki DB aur Booking History mein exact time save ho.

            var bookingData = new
            {
                resourceId = arenaId,
                playDate = playDate,
                startTime = startTime,
                endTime = endTime,
                bookingMode = bookingMode,
                tournamentPackage = string.IsNullOrWhiteSpace(tournamentPackage) ? null : tournamentPackage,
                paymentMode = paymentMode,
                amount = totalBill,        // 🌟 FORCE Exact Price
                totalBill = totalBill      // 🌟 FORCE Exact Price
            };

            var content = new StringContent(JsonSerializer.Serialize(bookingData), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("api/Booking/create", content);

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                var apiResult = JsonSerializer.Deserialize<BookingCreateResponse>(responseString, _jsonOptions);

                if (apiResult != null && apiResult.RequiresPayment)
                {
                    TempData["OrderId"] = apiResult.RazorpayOrderId;
                    TempData["Amount"] = apiResult.Amount.ToString();
                    TempData["BookingId"] = apiResult.BookingId;

                    return RedirectToAction("Payment");
                }

                TempData["Success"] = "Your turf booking has been successfully confirmed!";
                return RedirectToAction("Index", "BookingHistory");
            }
            else
            {
                var errorJson = await response.Content.ReadAsStringAsync();
                return Content($"<h1 style='color:red;'>🚨 API ERROR</h1><h2>Status Code: {response.StatusCode}</h2><p><b>Error Details:</b> {errorJson}</p><br><a href='javascript:history.back()'>Go Back</a>", "text/html");
            }
        }

        [HttpGet]
        public IActionResult Payment()
        {
            if (TempData["OrderId"] == null) return RedirectToAction("Index", "Explore");

            ViewBag.OrderId = TempData["OrderId"]?.ToString();
            ViewBag.Amount = Convert.ToDecimal(TempData["Amount"]);
            ViewBag.BookingId = Convert.ToInt32(TempData["BookingId"]);

            TempData.Keep();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> VerifyPayment(int bookingId, string paymentId, string orderId, string signature)
        {
            var token = Request.Cookies["JWToken"];
            if (string.IsNullOrEmpty(token))
            {
                TempData["Error"] = "Session expired. Please login again.";
                return RedirectToAction("Login", "Account");
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var verifyData = new { BookingId = bookingId, RazorpayPaymentId = paymentId, RazorpayOrderId = orderId, RazorpaySignature = signature };
            var content = new StringContent(JsonSerializer.Serialize(verifyData), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("api/Booking/verify", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Payment Verified & Booking Confirmed Successfully! 🏆";
            }
            else
            {
                TempData["Error"] = "Payment Verification Failed! Please contact support.";
            }

            return RedirectToAction("Index", "BookingHistory");
        }
    }

    public class BookedTimesResponse
    {
        public bool Success { get; set; }
        public List<BookedTimeRange>? Data { get; set; }
    }

    public class BookedTimeRange
    {
        public string Start { get; set; } = string.Empty;
        public string End { get; set; } = string.Empty;
    }

    public class BookingCreateResponse
    {
        public string? Message { get; set; }
        public int BookingId { get; set; }
        public bool RequiresPayment { get; set; }
        public string? RazorpayOrderId { get; set; }
        public decimal Amount { get; set; }
        public decimal TotalBill { get; set; }
    }
}