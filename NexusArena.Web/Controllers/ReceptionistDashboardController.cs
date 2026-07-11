using Microsoft.AspNetCore.Mvc;
using NexusArena.Web.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NexusArena.Web.Controllers
{
    public class ReceptionistDashboardController : Controller
    {
        private readonly HttpClient _httpClient;

        public ReceptionistDashboardController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IActionResult> Index()
        {
            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;

            using (var client = new HttpClient(handler))
            {
                var token = HttpContext.Request.Cookies["JWToken"];
                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                try
                {
                    // 🌟 FIX: Clean URL
                    HttpResponseMessage response = await client.GetAsync("http://localhost:5092/api/Receptionist/GetLiveDashboard");

                    if (response.IsSuccessStatusCode)
                    {
                        string data = await response.Content.ReadAsStringAsync();
                        var model = System.Text.Json.JsonSerializer.Deserialize<ReceptionistDashboardViewModel>(data, new System.Text.Json.JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                        return View(model);
                    }
                    else
                    {
                        ViewBag.ErrorMessage = $"🚨 API Error: {response.StatusCode}";
                        return View(new ReceptionistDashboardViewModel());
                    }
                }
                catch (System.Exception ex)
                {
                    ViewBag.ErrorMessage = $"🚨 Connection Crash! Error: {ex.Message}";
                    return View(new ReceptionistDashboardViewModel());
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateWalkInBooking(string customerName, string customerPhone, int resourceId, string startTime, string endTime)
        {
            try
            {
                var handler = new HttpClientHandler();
                handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;

                using (var client = new HttpClient(handler))
                {
                    var token = HttpContext.Request.Cookies["JWToken"];
                    if (!string.IsNullOrEmpty(token))
                    {
                        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                    }

                    var payload = new
                    {
                        CustomerName = customerName,
                        CustomerPhone = customerPhone,
                        ResourceId = resourceId,
                        StartTime = startTime,
                        EndTime = endTime,
                        BookingDate = DateTime.Today.ToString("yyyy-MM-dd")
                    };

                    var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");
                    // 🌟 FIX: Clean URL
                    HttpResponseMessage response = await client.PostAsync("http://localhost:5092/api/Receptionist/walk-in-booking", content);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["SuccessMessage"] = "✅ Walk-in Booking successfully created! Customer data saved to database.";
                    }
                    else
                    {
                        string errorMsg = await response.Content.ReadAsStringAsync();
                        TempData["ErrorMessage"] = $"❌ Booking failed! {errorMsg}";
                    }
                }
            }
            catch (System.Exception ex)
            {
                TempData["ErrorMessage"] = "Connection Error: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(int bookingId)
        {
            try
            {
                var handler = new HttpClientHandler();
                handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;

                using (var client = new HttpClient(handler))
                {
                    var token = HttpContext.Request.Cookies["JWToken"];
                    if (!string.IsNullOrEmpty(token))
                    {
                        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                    }

                    var content = new StringContent(System.Text.Json.JsonSerializer.Serialize("Completed"), System.Text.Encoding.UTF8, "application/json");
                    // 🌟 FIX: Clean URL
                    HttpResponseMessage response = await client.PutAsync($"http://localhost:5092/api/Receptionist/update-status/{bookingId}", content);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["SuccessMessage"] = $"Booking #{bookingId} successfully checked out! 🏏";
                        Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = $"Checkout failed. Status: {response.StatusCode}";
                    }
                }
            }
            catch (System.Exception ex)
            {
                TempData["ErrorMessage"] = "Connection Error: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Bookings()
        {
            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;

            using (var client = new HttpClient(handler))
            {
                var token = HttpContext.Request.Cookies["JWToken"];
                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                try
                {
                    // 🌟 FIX: Clean URL
                    HttpResponseMessage response = await client.GetAsync("http://localhost:5092/api/Receptionist/booking-history");

                    if (response.IsSuccessStatusCode)
                    {
                        string data = await response.Content.ReadAsStringAsync();
                        var history = System.Text.Json.JsonSerializer.Deserialize<List<NexusArena.Web.Models.ReceptionistBookingHistoryViewModel>>(data, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        return View(history);
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "An error occurred while retrieving the data!";
                    }
                }
                catch (System.Exception ex)
                {
                    ViewBag.ErrorMessage = "API Connection Error: " + ex.Message;
                }
            }

            return View(new List<NexusArena.Web.Models.ReceptionistBookingHistoryViewModel>());
        }

        [HttpPost]
        public async Task<IActionResult> CollectPayment(int bookingId)
        {
            try
            {
                var handler = new HttpClientHandler();
                handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;

                using (var client = new HttpClient(handler))
                {
                    var token = HttpContext.Request.Cookies["JWToken"];
                    if (!string.IsNullOrEmpty(token))
                    {
                        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                    }

                    var content = new StringContent("", System.Text.Encoding.UTF8, "application/json");
                    // 🌟 FIX: Clean URL
                    HttpResponseMessage response = await client.PutAsync($"http://localhost:5092/api/Receptionist/collect-payment/{bookingId}", content);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["SuccessMessage"] = $"Got the money! Booking #{bookingId} The payment has been cleared. 💸";
                        Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = $"The payment failed. Status: {response.StatusCode}";
                    }
                }
            }
            catch (System.Exception ex)
            {
                TempData["ErrorMessage"] = "Connection Error: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> AvailableTurfs()
        {
            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;

            using (var client = new HttpClient(handler))
            {
                var token = HttpContext.Request.Cookies["JWToken"];
                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                try
                {
                    // 🌟 FIX: Clean URL
                    HttpResponseMessage response = await client.GetAsync("http://localhost:5092/api/Receptionist/available-turfs");

                    if (response.IsSuccessStatusCode)
                    {
                        string data = await response.Content.ReadAsStringAsync();
                        var turfs = System.Text.Json.JsonSerializer.Deserialize<List<NexusArena.Web.Models.AvailableTurfViewModel>>(data, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        return View(turfs);
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "API Error: Data could not be retrieved.";
                    }
                }
                catch (System.Exception ex)
                {
                    ViewBag.ErrorMessage = "Connection Error: " + ex.Message;
                }
            }

            return View(new List<NexusArena.Web.Models.AvailableTurfViewModel>());
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var token = Request.Cookies["JWToken"];
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Account");

            ViewBag.Name = "Sahil Mirza";
            ViewBag.Email = "sahilmirza@nexus.com";
            ViewBag.Phone = "+91 9876543210";
            ViewBag.Role = "Receptionist";
            ViewBag.Branch = "Surat Arena";

            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                // 🌟 FIX: Clean URL
                string apiUrl = "http://localhost:5092/api/Auth/GetProfile";

                var response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    using JsonDocument doc = JsonDocument.Parse(responseData);
                    var root = doc.RootElement;

                    if (root.TryGetProperty("data", out var dataObj) && dataObj.ValueKind == JsonValueKind.Object)
                    {
                        root = dataObj;
                    }

                    if (root.TryGetProperty("fullName", out var n) && !string.IsNullOrWhiteSpace(n.GetString())) ViewBag.Name = n.GetString();
                    else if (root.TryGetProperty("FullName", out var n2) && !string.IsNullOrWhiteSpace(n2.GetString())) ViewBag.Name = n2.GetString();

                    if (root.TryGetProperty("email", out var e) && !string.IsNullOrWhiteSpace(e.GetString())) ViewBag.Email = e.GetString();
                    else if (root.TryGetProperty("Email", out var e2) && !string.IsNullOrWhiteSpace(e2.GetString())) ViewBag.Email = e2.GetString();

                    if (root.TryGetProperty("phone", out var p) && !string.IsNullOrWhiteSpace(p.GetString())) ViewBag.Phone = p.GetString();
                    else if (root.TryGetProperty("Phone", out var p2) && !string.IsNullOrWhiteSpace(p2.GetString())) ViewBag.Phone = p2.GetString();

                    if (root.TryGetProperty("roleName", out var r) && !string.IsNullOrWhiteSpace(r.GetString())) ViewBag.Role = r.GetString();
                    else if (root.TryGetProperty("RoleName", out var r2) && !string.IsNullOrWhiteSpace(r2.GetString())) ViewBag.Role = r2.GetString();

                    if (root.TryGetProperty("branch", out var b) && !string.IsNullOrWhiteSpace(b.GetString())) ViewBag.Branch = b.GetString();
                    else if (root.TryGetProperty("Branch", out var b2) && !string.IsNullOrWhiteSpace(b2.GetString())) ViewBag.Branch = b2.GetString();
                }
                else
                {
                    ViewBag.ApiDebugError = $"API Error: Status Code {response.StatusCode} | URL Tried: {apiUrl}";
                }
            }
            catch (Exception ex)
            {
                ViewBag.ApiDebugError = $"Connection Exception: {ex.Message}";
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(string fullName, string phoneNumber)
        {
            var token = Request.Cookies["JWToken"];
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Account");

            var updateData = new { FullName = fullName, Phone = phoneNumber };
            var content = new StringContent(JsonSerializer.Serialize(updateData), Encoding.UTF8, "application/json");

            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                // 🌟 FIX: Clean URL
                var response = await _httpClient.PostAsync("http://localhost:5092/api/Auth/UpdateProfile", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Your profile details have been updated successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to update profile. Please check the details and try again.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "API Connection Error: " + ex.Message;
            }

            return RedirectToAction("Profile");
        }
    }
}