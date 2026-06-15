using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticAssets;
using System.Text.Json;
using NexusArena.Web.Models;

namespace NexusArena.Web.Controllers
{
    public class SuperAdminDashboardController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public SuperAdminDashboardController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient();
            string apiUrl = "http://localhost:5092/api/Dashboard/Stats";
            var dashboardData = new DashboardStatsViewModel();

            try
            {
                HttpResponseMessage response = await client.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    string jsonData = await response.Content.ReadAsStringAsync();
                    dashboardData = JsonSerializer.Deserialize<DashboardStatsViewModel>(jsonData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                else
                {
                    ViewBag.Error = "Failed to fetch data from the API. Status code: " + response.StatusCode;
                    SetDefaultStats(dashboardData);
                }
            }
            catch (Exception)
            {
                ViewBag.Error = "The API is completely inaccessible. Please check if API project is running.";
                SetDefaultStats(dashboardData);
            }
            return View(dashboardData);
        }

        private void SetDefaultStats(DashboardStatsViewModel model)
        {
            model.TotalPlayers = 0;
            model.RegisteredOwners = 0;
            model.ActiveArenas = 0;
            model.PlatformRevenue = "₹0";
        }
    }
}
