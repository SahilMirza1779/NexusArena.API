using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using NexusArena.MVC.Models;

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

        [HttpGet]
        public IActionResult ManageResources() => View(new ResourceViewModel());

        [HttpPost]
        public async Task<IActionResult> AddResource(ResourceViewModel model)
        {
            using (var client = GetAuthenticatedClient())
            {
                var payload = new { ResourceName = model.ResourceName, Capacity = model.Capacity };
                var json = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"{_baseApiUrl}ResourceManager/add", json);

                if (response.IsSuccessStatusCode)
                    return RedirectToAction("ResourceList");

                ViewBag.Error = "Error: " + response.StatusCode;
            }
            return View("ManageResources", model);
        }

        [HttpGet]
        public async Task<IActionResult> ResourceList()
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
    }
}