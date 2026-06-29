using Microsoft.AspNetCore.Mvc;
using NexusArena.Web.Models;
using System.Net.Http;
using System.Net.Http.Headers; 
using System.Text.Json;
using System.Threading.Tasks;

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
    }
}