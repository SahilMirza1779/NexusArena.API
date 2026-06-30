using Microsoft.AspNetCore.Mvc;
using NexusArena.MVC.Models;
using NexusArena.Web.Models;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NexusArena.Web.Controllers
{
    public class OwnerDashboardController : Controller
    {
        private readonly string _baseApiUrl = "http://localhost:5092/api/";

        private HttpClient GetAuthenticatedClient()
        {
            var client = new HttpClient();
            string? token = Request.Cookies["JWToken"];
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        // ================= DASHBOARD INDEX =========================
        [HttpGet]
        public IActionResult Index()
        {
            // Dummy data for testing UI
            var viewModel = new OwnerDashboardViewModel
            {
                TodayRevenue = 5400,
                LiveOccupancy = "75%",
                UpcomingBookings = new List<UpcomingBookingViewModel>
                {
                    new UpcomingBookingViewModel { BookingId = 1, CustomerName = "Rahul Sharma", FacilityName = "Premium Box Cricket", TimeSlot = "04:00 PM - 05:00 PM", Status = "Confirmed" },
                    new UpcomingBookingViewModel { BookingId = 2, CustomerName = "Aman Verma", FacilityName = "Pool Table 1", TimeSlot = "05:30 PM - 06:30 PM", Status = "Pending" },
                    new UpcomingBookingViewModel { BookingId = 3, CustomerName = "Zubair Khan", FacilityName = "Pickleball Court", TimeSlot = "07:00 PM - 08:00 PM", Status = "Confirmed" }
                }
            };

            ViewBag.Error = "Note: API data is hidden. Currently showing dummy data for UI testing.";

            return View(viewModel);
        }

        // ================= CANCEL BOOKING BUTTON LOGIC =========================
        [HttpPost]
        public async Task<IActionResult> CancelBooking(int bookingId, string cancelReason)
        {
            // Yahan hum cancelReason receive kar rahe hain! 
            // Jab real API hogi toh hum is reason ko API me bhejenge.

            // using (var client = GetAuthenticatedClient()) {
            //    var payload = new { BookingId = bookingId, Reason = cancelReason };
            //    var json = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            //    var response = await client.PostAsync($"{_baseApiUrl}Booking/cancel", json);
            // }

            // Success message mein reason dikhayenge testing ke liye
            TempData["Success"] = $"Booking #{bookingId} cancelled successfully! Reason: {cancelReason}";
            return RedirectToAction("Index");
        }

        // ================= MANAGE RESOURCES =========================
        [HttpGet]
        public async Task<IActionResult> ManageResources()
        {
            List<ResourceViewModel> list = new List<ResourceViewModel>();
            using (var client = GetAuthenticatedClient())
            {
                var response = await client.GetAsync($"{_baseApiUrl}ResourceManager/GetAllFacilities");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    list = JsonSerializer.Deserialize<List<ResourceViewModel>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<ResourceViewModel>();
                }
            }
            ViewBag.ResourceList = list;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddResource(ResourceViewModel model)
        {
            using (var client = GetAuthenticatedClient())
            {
                var payload = new { ResourceName = model.ResourceName, ResourceType = model.ResourceType, Capacity = model.Capacity, BasePricePerHour = model.BasePricePerHour };
                var json = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"{_baseApiUrl}ResourceManager/add", json);
                if (response.IsSuccessStatusCode) return RedirectToAction("ManageResources");
            }
            return RedirectToAction("ManageResources");
        }

        // ================= PRICING & SLOTS =========================
        [HttpGet]
        public async Task<IActionResult> PricingAndSlots()
        {
            var viewModel = new PricingAndSlotsPageViewModel();
            using (var client = GetAuthenticatedClient())
            {
                var resResponse = await client.GetAsync($"{_baseApiUrl}ResourceManager/GetAllFacilities");
                if (resResponse.IsSuccessStatusCode)
                {
                    var json = await resResponse.Content.ReadAsStringAsync();
                    viewModel.Resources = JsonSerializer.Deserialize<List<ResourceViewModel>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<ResourceViewModel>();
                }

                var slotResponse = await client.GetAsync($"{_baseApiUrl}TimeSlot/GetAll");
                if (slotResponse.IsSuccessStatusCode)
                {
                    var json = await slotResponse.Content.ReadAsStringAsync();
                    viewModel.TimeSlots = JsonSerializer.Deserialize<List<TimeSlotViewModel>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<TimeSlotViewModel>();
                }
            }
            return View(viewModel);
        }

        // ================= STAFF / ARENAS ====================
        [HttpGet]
        public async Task<IActionResult> Staff()
        {
            var viewModel = new StaffPageViewModel();
            using (var client = GetAuthenticatedClient())
            {
                var response = await client.GetAsync($"{_baseApiUrl}Staff/GetAll");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    viewModel.StaffList = JsonSerializer.Deserialize<List<ManageReceptionistViewModel>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<ManageReceptionistViewModel>();
                }
            }
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> ManageArenas()
        {
            var viewModel = new ManageArenaPageViewModel();
            using (var client = GetAuthenticatedClient())
            {
                var response = await client.GetAsync($"{_baseApiUrl}Arena/GetMyArenas");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    viewModel.ArenasList = JsonSerializer.Deserialize<List<NexusArena.Web.Models.ArenaViewModel>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<NexusArena.Web.Models.ArenaViewModel>();
                }
            }
            return View(viewModel);
        }
    }
}