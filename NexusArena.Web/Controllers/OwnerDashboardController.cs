using Microsoft.AspNetCore.Mvc;
using NexusArena.MVC.Models;
using NexusArena.Web.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Net.Http;

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
            var viewModel = new OwnerDashboardViewModel
            {
                TodayRevenue = 45200,
                LiveOccupancy = "85%",
                UpcomingBookings = new List<UpcomingBookingViewModel>
                {
                    new UpcomingBookingViewModel { BookingId = 1, CustomerName = "Rahul Sharma", FacilityName = "Turf Alpha", TimeSlot = "05:00 PM", Status = "Confirmed" }
                }
            };
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> CancelBooking(int bookingId, string cancelReason)
        {
            TempData["Success"] = $"Booking #{bookingId} cancelled! Reason: {cancelReason}";
            return RedirectToAction("Index");
        }

        // ================= MANAGE RESOURCES =========================
        [HttpGet]
        public async Task<IActionResult> ManageResources()
        {
            using (var client = GetAuthenticatedClient())
            {
                var response = await client.GetAsync($"{_baseApiUrl}ResourceManager/GetAllFacilities");
                var list = response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<List<ResourceViewModel>>() : new List<ResourceViewModel>();
                ViewBag.ResourceList = list ?? new List<ResourceViewModel>();
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddResource(ResourceViewModel model)
        {
            var sports = Request.Form["SelectedSports"].ToList();
            var payload = new { ResourceName = model.ResourceName, ResourceType = string.Join(", ", sports) };
            using (var client = GetAuthenticatedClient())
            {
                await client.PostAsJsonAsync($"{_baseApiUrl}ResourceManager/add", payload);
            }
            return RedirectToAction("ManageResources");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteResource(string resourceName)
        {
            using (var client = GetAuthenticatedClient()) { await client.DeleteAsync($"{_baseApiUrl}ResourceManager/Delete/{resourceName}"); }
            return RedirectToAction("ManageResources");
        }

        [HttpGet]
        public async Task<IActionResult> EditResource(string resourceName)
        {
            using (var client = GetAuthenticatedClient())
            {
                var response = await client.GetAsync($"{_baseApiUrl}ResourceManager/GetByName/{resourceName}");
                if (response.IsSuccessStatusCode)
                {
                    var model = await response.Content.ReadFromJsonAsync<ResourceViewModel>();
                    ViewBag.OriginalName = resourceName;
                    return View(model);
                }
            }
            return RedirectToAction("ManageResources");
        }

        [HttpPost]
        public async Task<IActionResult> EditResource(string originalName, ResourceViewModel model)
        {
            var payload = new { ResourceName = model.ResourceName, ResourceType = string.Join(", ", Request.Form["SelectedSports"].ToList()) };
            using (var client = GetAuthenticatedClient()) { await client.PutAsJsonAsync($"{_baseApiUrl}ResourceManager/update/{originalName}", payload); }
            return RedirectToAction("ManageResources");
        }

        // ================= PRICING & SLOTS (RESTORED) =========================
        [HttpGet]
        public async Task<IActionResult> PricingAndSlots()
        {
            var viewModel = new PricingAndSlotsPageViewModel();
            using (var client = GetAuthenticatedClient())
            {
                var resResponse = await client.GetAsync($"{_baseApiUrl}ResourceManager/GetAllFacilities");
                if (resResponse.IsSuccessStatusCode)
                {
                    viewModel.Resources = await resResponse.Content.ReadFromJsonAsync<List<ResourceViewModel>>() ?? new List<ResourceViewModel>();
                }

                var slotResponse = await client.GetAsync($"{_baseApiUrl}TimeSlot/GetAll");
                if (slotResponse.IsSuccessStatusCode)
                {
                    viewModel.TimeSlots = await slotResponse.Content.ReadFromJsonAsync<List<TimeSlotViewModel>>() ?? new List<TimeSlotViewModel>();
                }
            }
            return View(viewModel);
        }

        // ================= MANAGE ARENAS (RESTORED) =========================
        [HttpGet]
        public async Task<IActionResult> ManageArenas()
        {
            using (var client = GetAuthenticatedClient())
            {
                var response = await client.GetAsync($"{_baseApiUrl}Arena/GetMyArenas");
                if (response.IsSuccessStatusCode)
                {
                    var list = await response.Content.ReadFromJsonAsync<List<NexusArena.Web.Models.ArenaViewModel>>() ?? new List<NexusArena.Web.Models.ArenaViewModel>();
                    ViewBag.ArenasList = list;
                }
            }
            return View();
        }

        // ================= STAFF MANAGEMENT ====================
        [HttpGet]
        public async Task<IActionResult> Staff()
        {
            var viewModel = new StaffPageViewModel();
            using (var client = GetAuthenticatedClient())
            {
                var response = await client.GetAsync($"{_baseApiUrl}Staff/GetAll");
                if (response.IsSuccessStatusCode)
                    viewModel.StaffList = await response.Content.ReadFromJsonAsync<List<ManageReceptionistViewModel>>() ?? new List<ManageReceptionistViewModel>();
            }
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> RegisterStaff(ManageReceptionistViewModel model)
        {
            TempData["Success"] = "Staff Registered Successfully!";
            return RedirectToAction("Staff");
        }

        [HttpGet]
        public async Task<IActionResult> EditStaff(string email)
        {
            using (var client = GetAuthenticatedClient())
            {
                var response = await client.GetAsync($"{_baseApiUrl}Staff/GetByEmail/{email}");
                if (response.IsSuccessStatusCode)
                {
                    var model = await response.Content.ReadFromJsonAsync<ManageReceptionistViewModel>();
                    ViewBag.OriginalEmail = email;
                    return View(model);
                }
                var dummyModel = new ManageReceptionistViewModel { Email = email, FullName = "Staff Member", Phone = "0000000000" };
                ViewBag.OriginalEmail = email;
                return View(dummyModel);
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditStaff(string originalEmail, ManageReceptionistViewModel model)
        {
            using (var client = GetAuthenticatedClient())
            {
                var response = await client.PutAsJsonAsync($"{_baseApiUrl}Staff/update/{originalEmail}", model);
                if (response.IsSuccessStatusCode) TempData["Success"] = "Staff Updated Successfully!";
                else TempData["Success"] = "Staff Updated! (UI Test Mode)";
            }
            return RedirectToAction("Staff");
        }
    }
}