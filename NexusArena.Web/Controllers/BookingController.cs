using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;

namespace NexusArena.Web.Controllers
{
    public class BookingController : Controller
    {
        private readonly HttpClient _httpClient;

        public BookingController()
        {
            _httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:5092/") };
        }

        public async Task<IActionResult> CheckSlots(int id, string? date)
        {
            var token = Request.Cookies["JWToken"];
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Account");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            string selectedDate = string.IsNullOrEmpty(date) ? DateTime.Now.ToString("yyyy-MM-dd") : date;

            ViewBag.ArenaId = id;
            ViewBag.SelectedDate = selectedDate;

            var slots = new List<SlotViewModel>();

            try
            {
                var response = await _httpClient.GetAsync($"api/Booking/available-slots?arenaId={id}&date={selectedDate}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<SlotApiResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    slots = result?.data ?? new List<SlotViewModel>();
                }
            }
            catch (Exception ex) { ViewBag.Error = ex.Message; }

            return View(slots);
        }

        public IActionResult Review(int arenaId, string date, int slotId, string startTime, string endTime, decimal price)
        {
            ViewBag.ArenaId = arenaId;
            ViewBag.Date = date;
            ViewBag.SlotId = slotId;
            ViewBag.StartTime = startTime;
            ViewBag.EndTime = endTime;
            ViewBag.Price = price;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Confirm(int arenaId, int slotId, string playDate)
        {
            var token = Request.Cookies["JWToken"];
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var bookingData = new { ArenaId = arenaId, SlotId = slotId, PlayDate = playDate };
            var content = new StringContent(JsonSerializer.Serialize(bookingData), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("api/Booking/create", content);

            if (response.IsSuccessStatusCode) return RedirectToAction("Index", "Explore");

            TempData["ErrorMessage"] = "Booking Failed!";
            return RedirectToAction("CheckSlots", new { id = arenaId });
        }
    }

    public class SlotApiResponse { public List<SlotViewModel>? data { get; set; } }
    public class SlotViewModel { public int slotId { get; set; } public string? startTime { get; set; } public string? endTime { get; set; } public decimal price { get; set; } public bool isAvailable { get; set; } }
}