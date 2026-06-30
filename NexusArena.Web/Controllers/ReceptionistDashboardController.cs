using Microsoft.AspNetCore.Mvc;
using NexusArena.Web.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using System;

namespace NexusArena.Web.Controllers
{
    public class ReceptionistDashboardController : Controller
    {
        private readonly HttpClient _httpClient;

        private readonly string _apiUrl = "https://localhost:5092/api/Receptionist";

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
        public async Task<IActionResult> CreateWalkInBooking(int customerId, int resourceId, int slotId)
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
                        CustomerId = customerId,
                        ResourceId = resourceId,
                        SlotId = slotId,
                        BookingDate = DateTime.Today.ToString("yyyy-MM-dd")
                    };

                    var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync("http://localhost:5092/api/Receptionist/walk-in-booking", content);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["SuccessMessage"] = "Walk-in Booking successfully created! 🎉";
                    }
                    else
                    {
                        string errorMsg = await response.Content.ReadAsStringAsync();
                        TempData["ErrorMessage"] = $"Booking failed! Status: {response.StatusCode}. The API says: {errorMsg}";
                    }
                }
            }
            catch (System.Exception ex)
            {
                TempData["ErrorMessage"] = "Connection Crash! Error: " + ex.Message;
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
                    HttpResponseMessage response = await client.PutAsync($"http://localhost:5092/api/Receptionist/update-status/{bookingId}", content);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["SuccessMessage"] = $"Booking #{bookingId} successfully checked out! 🏏";

                        Response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");
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
                    HttpResponseMessage response = await client.PutAsync($"http://localhost:5092/api/Receptionist/collect-payment/{bookingId}", content);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["SuccessMessage"] = $"Got the money! Booking #{bookingId} The payment has been cleared. 💸";

                        Response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");
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
            return View();
        }
    }
}