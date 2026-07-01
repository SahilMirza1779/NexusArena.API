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

            var slots = new List<SlotViewModel>();

            try
            {
                var response = await _httpClient.GetAsync($"api/Booking/available-slots?arenaId={arenaId}&date={selectedDate}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<SlotApiResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    slots = result?.data ?? new List<SlotViewModel>();
                }
                else
                {
                    ViewBag.Error = "Failed to load slots from API.";
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
            }

            return View(slots);
        }

        // 🌟 THE FIX: Yahan 'timeDisplay' le rahe hain taaki naya flow kaam kare bina purane Review page ko tode
        [HttpGet]
        public IActionResult Review(int arenaId, string date, int slotId, string timeDisplay, decimal price)
        {
            ViewBag.ArenaId = arenaId;
            ViewBag.Date = date;
            ViewBag.SlotId = slotId;
            ViewBag.Price = price;

            // TimeDisplay ("10:00 AM - 11:00 AM") ko split karke purane ViewBag me bhej rahe hain
            if (!string.IsNullOrEmpty(timeDisplay) && timeDisplay.Contains("-"))
            {
                var parts = timeDisplay.Split('-');
                ViewBag.StartTime = parts[0].Trim();
                ViewBag.EndTime = parts[1].Trim();
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Confirm(int arenaId, int slotId, string playDate, string paymentMode)
        {
            var token = Request.Cookies["JWToken"];
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Account");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var bookingData = new { ArenaId = arenaId, SlotId = slotId, PlayDate = playDate, PaymentMode = paymentMode };
            var content = new StringContent(JsonSerializer.Serialize(bookingData), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("api/Booking/create", content);

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                var apiResult = JsonSerializer.Deserialize<BookingCreateResponse>(responseString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (apiResult != null && apiResult.requiresPayment)
                {
                    TempData["OrderId"] = apiResult.razorpayOrderId;
                    TempData["Amount"] = apiResult.amount.ToString();
                    TempData["BookingId"] = apiResult.bookingId;

                    return RedirectToAction("Payment");
                }

                TempData["Success"] = "Your turf booking has been successfully confirmed!";
                return RedirectToAction("Index", "BookingHistory");
            }
            else
            {
                var errorString = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = $"System Error: {errorString}";
                return RedirectToAction("SelectSlot", new { arenaId = arenaId });
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

    // =========================================================
    // 🌟 THE FIX: API ke variables aur Inke variables same kar diye hain
    // =========================================================
    public class SlotApiResponse
    {
        public List<SlotViewModel>? data { get; set; }
    }

    public class SlotViewModel
    {
        public int slotId { get; set; }
        public string? timeDisplay { get; set; } // API ab 'timeDisplay' bhej raha hai
        public decimal price { get; set; }
        public bool isBooked { get; set; }       // API ab 'isBooked' bhej raha hai
    }

    public class BookingCreateResponse
    {
        public string? message { get; set; }
        public int bookingId { get; set; }
        public bool requiresPayment { get; set; }
        public string? razorpayOrderId { get; set; }
        public decimal amount { get; set; }
    }
}