using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System;
using System.Net.Http;

namespace NexusArena.Web.Controllers
{
    public class NotificationController : Controller
    {
        private readonly HttpClient _httpClient;

        public NotificationController()
        {
            _httpClient = new HttpClient();
            // 🌟 THE FIX: Bracket virus removed. Clean URL applied!
            _httpClient.BaseAddress = new Uri("http://localhost:5092/");
        }

        [HttpGet]
        public async Task<IActionResult> Fetch()
        {
            var token = Request.Cookies["JWToken"];
            if (string.IsNullOrEmpty(token)) return Json(new { count = 0, data = Array.Empty<object>() });

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                // 🌟 NAYA: Naye safe API endpoint ko call kar rahe hain
                var response = await _httpClient.GetAsync("api/UserNotifications/my-notifications");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return Content(json, "application/json");
                }
            }
            catch { /* Silent fail */ }

            return Json(new { count = 0, data = Array.Empty<object>() });
        }
    }
}