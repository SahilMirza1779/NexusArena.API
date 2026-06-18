using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticAssets;
using System.Text.Json;
using System.Net.Http;
using NexusArena.Web.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

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

        [HttpGet]
        public async Task<IActionResult> Arenas()
        {
            var client = _httpClientFactory.CreateClient();

            string apiUrl = "http://localhost:5092/api/Arena/GetAll";

            var arenaList = new List<ArenaListViewModel>();

            try
            {
                HttpResponseMessage response = await client.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    string jsonData = await response.Content.ReadAsStringAsync();

                    var fetchedList = JsonSerializer.Deserialize<List<ArenaListViewModel>>(jsonData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (fetchedList != null)
                    {
                        arenaList = fetchedList;
                    }
                }
            }
            catch (Exception)
            {
            }

            return View(arenaList);
        }

        [HttpGet]
        public async Task<IActionResult> Manage(int id)
        {
            var client = _httpClientFactory.CreateClient();

            string apiUrl = $"http://localhost:5092/api/Arena/GetDetails/{id}";

            var arenaDetails = new ArenaDetailsViewModel();

            try
            {
                HttpResponseMessage response = await client.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    string jsonData = await response.Content.ReadAsStringAsync();
                    arenaDetails = JsonSerializer.Deserialize<ArenaDetailsViewModel>(jsonData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                else
                {
                    return RedirectToAction("Arenas");
                }
            }
            catch (Exception)
            {
                return RedirectToAction("Arenas");
            }

            return View(arenaDetails);
        }

        [HttpPost]
        public async Task<IActionResult> SuspendArena(int id)
        {
            var client = _httpClientFactory.CreateClient();
            string apiUrl = $"http://localhost:5092/api/Arena/Suspend/{id}";
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

        [HttpGet]
        public async Task<IActionResult> Categories()
        {
            var client = _httpClientFactory.CreateClient();
            string apiUrl = "http://localhost:5092/api/SportCategory/GetAll";
            var categoryList = new List<CategoryViewModel>();

            try
            {
                HttpResponseMessage response = await client.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    string jsonData = await response.Content.ReadAsStringAsync();
                    var fetchedList = System.Text.Json.JsonSerializer.Deserialize<List<CategoryViewModel>>(jsonData, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (fetchedList != null)
                    {
                        categoryList = fetchedList;
                    }
                }
            }
            catch (Exception)
            {

            }

            return View(categoryList);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CategoryViewModel newCategory)
        {
            var client = _httpClientFactory.CreateClient();
            string apiUrl = "http://localhost:5092/api/SportCategory/Create";
            var jsonContent = new StringContent(System.Text.Json.JsonSerializer.Serialize(newCategory), System.Text.Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = await client.PostAsync(apiUrl, jsonContent);
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

        [HttpPost]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var client = _httpClientFactory.CreateClient();
            string apiUrl = $"http://localhost:5092/api/SportCategory/Delete/{id}";

            try
            {
                HttpResponseMessage response = await client.DeleteAsync(apiUrl);
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

        [HttpPost]
        public async Task<IActionResult> EditCategory(int id, [FromBody] CategoryViewModel upadatedCategory)
        {
            var client = _httpClientFactory.CreateClient();
            string apiUrl = $"http://localhost:5092/api/SportCategory/Update/{id}";
            var jsonContent = new StringContent(System.Text.Json.JsonSerializer.Serialize(upadatedCategory), System.Text.Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = await client.PutAsync(apiUrl, jsonContent);
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

        [HttpGet]
        public async Task<IActionResult> Owners()
        {
            var client = _httpClientFactory.CreateClient();

            string apiUrl = "http://localhost:5092/api/ManageOwners/GetAll";

            var ownerList = new List<ManageOwnerViewModel>();

            try
            {
                HttpResponseMessage response = await client.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    string jsonData = await response.Content.ReadAsStringAsync();

                    var fetchedList = System.Text.Json.JsonSerializer.Deserialize<List<ManageOwnerViewModel>>(jsonData, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (fetchedList != null)
                    {
                        ownerList = fetchedList;
                    }
                }
            }
            catch (Exception)
            {

            }

            return View(ownerList);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleOwnerStatus(int id)
        {
            var client = _httpClientFactory.CreateClient();
            string apiUrl = $"http://localhost:5092/api/ManageOwners/ToggleStatus/{id}";
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

        [HttpGet]
        public async Task<IActionResult> Users()
        {
            var client = _httpClientFactory.CreateClient();
            string apiUrl = "http://localhost:5092/api/ManageUsers/GetAll";
            var userList = new List<ManageUserViewModel>();

            try
            {
                HttpResponseMessage response = await client.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    string jsonData = await response.Content.ReadAsStringAsync();
                    var fetchedList = System.Text.Json.JsonSerializer.Deserialize<List<ManageUserViewModel>>(jsonData, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (fetchedList != null)
                    {
                        userList = fetchedList;
                    }
                }
            }
            catch (Exception)
            {

            }

            return View(userList);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleUserStatus(int id)
        {
            var client = _httpClientFactory.CreateClient();
            string apiUrl = $"http://localhost:5092/api/ManageUsers/ToggleStatus/{id}";
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

        [HttpGet]
        public async Task<IActionResult> Receptionists()
        {
            var client = _httpClientFactory.CreateClient();

            string apiUrl = "http://localhost:5092/api/ManageReceptionists/GetAll";
            var receptionistList = new List<ManageReceptionistViewModel>();

            try
            {
                HttpResponseMessage response = await client.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    string jsonData = await response.Content.ReadAsStringAsync();
                    var fetchedList = System.Text.Json.JsonSerializer.Deserialize<List<ManageReceptionistViewModel>>(jsonData, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (fetchedList != null)
                    {
                        receptionistList = fetchedList;
                    }
                }
            }
            catch (Exception)
            {

            }
            return View(receptionistList);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleReceptionistStatus(int id)
        {
            var client = _httpClientFactory.CreateClient();
            string apiUrl = $"http://localhost:5092/api/ManageReceptionists/ToggleStatus/{id}";
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

        [HttpGet]
        public async Task<IActionResult> Reviews()
        {
            var client = _httpClientFactory.CreateClient();
            string apiUrl = "http://localhost:5092/api/ManageReviews/GetAll";

            var reviewList = new List<ManageReviewViewModel>();

            try
            {
                HttpResponseMessage response = await client.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    string jsonData = await response.Content.ReadAsStringAsync();
                    var fetchedList = System.Text.Json.JsonSerializer.Deserialize<List<ManageReviewViewModel>>(jsonData, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (fetchedList != null)
                    {
                        reviewList = fetchedList;
                    }
                }
            }
            catch (Exception)
            {

            }

            return View(reviewList);
        }

        [HttpPost]
        public async Task<IActionResult> DeletePlatformReview(int id)
        {
            var client = _httpClientFactory.CreateClient();
            string apiUrl = $"http://localhost:5092/api/ManageReviews/Delete/{id}";

            try
            {
                HttpResponseMessage response = await client.DeleteAsync(apiUrl);
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