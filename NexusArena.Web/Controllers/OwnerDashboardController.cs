using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using NexusArena.MVC.Models;
using System.Collections.Generic;

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
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            return client;
        }

        public async Task<IActionResult> Index()
        {
            string? token = Request.Cookies["JWToken"];

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Account");
            }

            var dashboardModel = new OwnerDashboardViewModel();
            string apiUrl = $"{_baseApiUrl}OwnerDashboard/stats";

            using (var client = GetAuthenticatedClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        string jsonResponse = await response.Content.ReadAsStringAsync();
                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var fetchedData = JsonSerializer.Deserialize<OwnerDashboardViewModel>(jsonResponse, options);

                        if (fetchedData != null)
                        {
                            dashboardModel = fetchedData;
                        }
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        Response.Cookies.Delete("JWToken");
                        return RedirectToAction("Login", "Account");
                    }
                    else
                    {
                        ViewBag.Error = $"API Error: {response.StatusCode}";
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.Error = "API Server se connect nahi ho paaya: " + ex.Message;
                }
            }

            return View(dashboardModel);
        }

        [HttpGet]
        public async Task<IActionResult> ManageResources()
        {
            string? token = Request.Cookies["JWToken"];
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Account");

            List<ResourceViewModel> resourceList = new List<ResourceViewModel>();

            using (var client = GetAuthenticatedClient())
            {
                var response = await client.GetAsync($"{_baseApiUrl}OwnerFacility/GetAllFacilities");

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    resourceList = JsonSerializer.Deserialize<List<ResourceViewModel>>(jsonResponse, options) ?? new List<ResourceViewModel>();
                }
            }

            ViewBag.ResourceList = resourceList;
            return View(new ResourceViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> AddResource(ResourceViewModel model)
        {
            if (!ModelState.IsValid) return View("ManageResources", model);

            using (var client = GetAuthenticatedClient())
            {
                var jsonContent = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"{_baseApiUrl}OwnerFacility/AddFacility", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Naya Resource successfully add ho gaya!";
                    return RedirectToAction("ManageResources");
                }

                ViewBag.Error = "Resource add karne me problem aayi.";
            }
            return View("ManageResources", model);
        }

        [HttpGet]
        public async Task<IActionResult> PricingAndSlots()
        {
            string? token = Request.Cookies["JWToken"];
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Account");

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Staff()
        {
            string? token = Request.Cookies["JWToken"];
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Account");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CancelBooking(int bookingId, string cancelReason)
        {
            using (var client = GetAuthenticatedClient())
            {
                var cancelData = new { BookingId = bookingId, Reason = cancelReason };
                var jsonContent = new StringContent(JsonSerializer.Serialize(cancelData), Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"{_baseApiUrl}Booking/Cancel", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Booking cancel ho gayi hai aur user ko message bhej diya gaya hai.";
                }
                else
                {
                    TempData["Error"] = "Cancellation fail ho gaya.";
                }
            }
            return RedirectToAction("Index");
        }
    }
}