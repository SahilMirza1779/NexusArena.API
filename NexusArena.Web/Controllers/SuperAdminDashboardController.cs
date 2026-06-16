using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticAssets;
using System.Text.Json;
using System.Net.Http;
using NexusArena.Web.Models;
using System.Collections.Generic;

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
            var dashboardData = new DashboardStatsViewModel();

            dashboardData.PendingApprovals = new List<PendingArenaViewModel>();

            string statsApiUrl = "http://localhost:5092/api/Dashboard/Stats";
            string arenasApiUrl = "http://localhost:5092/api/Dashboard/PendingArenas";

            try
            {
                HttpResponseMessage statsResponse = await client.GetAsync(statsApiUrl);
                if (statsResponse.IsSuccessStatusCode)
                {
                    string jsonData = await statsResponse.Content.ReadAsStringAsync();
                    dashboardData = JsonSerializer.Deserialize<DashboardStatsViewModel>(jsonData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (dashboardData.PendingApprovals == null)
                    {
                        dashboardData.PendingApprovals = new List<PendingArenaViewModel>();
                    }
                }
                else
                {
                    ViewBag.Error = "Failed to fetch stats from the API. Status code: " + statsResponse.StatusCode;
                    SetDefaultStats(dashboardData);
                }

                HttpResponseMessage arenasResponse = await client.GetAsync(arenasApiUrl);
                if (arenasResponse.IsSuccessStatusCode)
                {
                    string arenasJson = await arenasResponse.Content.ReadAsStringAsync();
                    var pendingList = JsonSerializer.Deserialize<List<PendingArenaViewModel>>(arenasJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (pendingList != null)
                    {
                        dashboardData.PendingApprovals = pendingList;
                    }
                }
            }
            catch (Exception)
            {
                ViewBag.Error = "The API is completely inaccessible. Please check if API project is running.";
                SetDefaultStats(dashboardData);
                dashboardData.PendingApprovals = new List<PendingArenaViewModel>(); 
            }

            return View(dashboardData);
        }

        private void SetDefaultStats(DashboardStatsViewModel model)
        {
            model.TotalPlayers = 0;
            model.RegisteredOwners = 0;
            model.TotalReceptionists = 0;
            model.ActiveArenas = 0;
            model.PlatformRevenue = "₹0";
        }

        [HttpPost]
        public async Task<IActionResult> ApproveArena(int id)
        {
            var client = _httpClientFactory.CreateClient();
            string apiUrl = $"http://localhost:5092/api/Dashboard/ApproveArena/{id}";

            var content = new StringContent("", System.Text.Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = await client.PostAsync(apiUrl, content);
                if (response.IsSuccessStatusCode)
                {
                    return Json(new { success = true });
                }
            }
            catch (Exception)
            {
            }

            return Json(new { success = false });
        }
    }
}