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

        public IActionResult Index() => View();

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
                // Yahan saari properties teri ResourceViewModel file ke mutabiq match kar di hain
                var payload = new
                {
                    ResourceName = model.ResourceName,
                    ResourceType = model.ResourceType,
                    Capacity = model.Capacity,
                    BasePricePerHour = model.BasePricePerHour,
                    Dimensions = model.Dimensions,
                    IncludedEquipment = model.IncludedEquipment,
                    Description = model.Description
                };

                var json = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"{_baseApiUrl}ResourceManager/add", json);

                if (response.IsSuccessStatusCode)
                    return RedirectToAction("ManageResources");

                ViewBag.Error = "Error: " + response.StatusCode;
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

        [HttpPost]
        public async Task<IActionResult> AddTimeSlot(TimeSlotViewModel NewSlot)
        {
            using (var client = GetAuthenticatedClient())
            {
                var payload = new { ResourceId = NewSlot.ResourceId, StartTime = NewSlot.StartTime, EndTime = NewSlot.EndTime, BasePrice = NewSlot.BasePrice, IsPremium = NewSlot.IsPremium };
                var json = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"{_baseApiUrl}TimeSlot/add", json);

                if (response.IsSuccessStatusCode) TempData["Success"] = "Time Slot successfully added!";
                else TempData["Error"] = "Failed to add Slot.";
            }
            return RedirectToAction("PricingAndSlots");
        }

        // ================= STAFF / RECEPTIONIST ====================
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

        [HttpPost]
        public async Task<IActionResult> AddStaff(ManageReceptionistViewModel NewStaff)
        {
            using (var client = GetAuthenticatedClient())
            {
                var payload = new { FullName = NewStaff.FullName, Email = NewStaff.Email, Phone = NewStaff.Phone, Password = NewStaff.Password };
                var json = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"{_baseApiUrl}Staff/add", json);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Staff Member successfully added!";
                }
                else
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    TempData["Error"] = $"Failed: {response.StatusCode} - {errorMsg}";
                }
            }
            return RedirectToAction("Staff");
        }

        // ================= MANAGE ARENAS ===========================
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

        [HttpPost]
        public async Task<IActionResult> AddArena(ManageArenaPageViewModel model)
        {
            using (var client = GetAuthenticatedClient())
            {
                var payload = new { Name = model.NewArena.Name, Location = model.NewArena.Location, City = model.NewArena.City };
                var json = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"{_baseApiUrl}Arena/add", json);

                if (response.IsSuccessStatusCode) TempData["Success"] = "Arena successfully added!";
                else TempData["Error"] = "Failed to add Arena.";
            }
            return RedirectToAction("ManageArenas");
        }
    }
}