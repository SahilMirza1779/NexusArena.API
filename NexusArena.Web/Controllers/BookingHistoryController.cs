using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;

namespace NexusArena.Web.Controllers
{
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public class BookingHistoryController : Controller
    {
        private readonly HttpClient _httpClient;

        public BookingHistoryController()
        {
            _httpClient = new HttpClient();
            // 🌟 DHAYAN RAHE: Agar aapka API port alag hai toh yahan update karein (e.g., 5092)
            _httpClient.BaseAddress = new Uri("http://localhost:5092/");
        }

        // ==========================================
        // 1. GET: MY BOOKINGS LIST
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var token = Request.Cookies["JWToken"];
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Account");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var viewModel = new List<BookingHistoryViewModel>();

            try
            {
                var response = await _httpClient.GetAsync("api/BookingHistory/my-history");
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var apiResult = JsonSerializer.Deserialize<BookingHistoryApiResponse>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    viewModel = apiResult?.data ?? new List<BookingHistoryViewModel>();
                }
                else
                {
                    ViewBag.Error = "Failed to fetch booking history.";
                }
            }
            catch (Exception ex) { ViewBag.Error = $"Connection Error: {ex.Message}"; }

            return View(viewModel);
        }

        // ==========================================
        // 2. POST: CANCEL BOOKING
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> Cancel(int bookingId)
        {
            var token = Request.Cookies["JWToken"];
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Account");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await _httpClient.PutAsync($"api/BookingHistory/cancel/{bookingId}", null);
                if (response.IsSuccessStatusCode) TempData["Success"] = "Your booking has been successfully cancelled.";
                else TempData["Error"] = "Failed to cancel the booking. Please try again.";
            }
            catch (Exception ex) { TempData["Error"] = $"Error: {ex.Message}"; }

            return RedirectToAction("Index");
        }

        // ==========================================
        // 3. GET: VIEW VIP TICKET (NAYA METHOD) 🎟️
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Ticket(int id)
        {
            var token = Request.Cookies["JWToken"];
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Account");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await _httpClient.GetAsync("api/BookingHistory/my-history");
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var apiResult = JsonSerializer.Deserialize<BookingHistoryApiResponse>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    // Pura data me se sirf wahi ticket nikalo jis par click hua hai
                    var ticketData = apiResult?.data?.FirstOrDefault(b => b.BookingId == id);

                    if (ticketData == null) return NotFound("Ticket details not found in your account!");

                    return View(ticketData);
                }
            }
            catch (Exception ex) { return Content($"System Error Loading Ticket: {ex.Message}"); }

            return RedirectToAction("Index");
        }
    }

    // 🌟 VIEW MODELS (Yahi par rahenge)
    public class BookingHistoryApiResponse
    {
        public string? message { get; set; }
        public List<BookingHistoryViewModel>? data { get; set; }
    }

    public class BookingHistoryViewModel
    {
        public int BookingId { get; set; }
        public string ArenaName { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string PlayDate { get; set; } = string.Empty;
        public string TimeSlot { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal PendingAmount { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}