using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;

namespace NexusArena.Web.Controllers
{
    public class ReviewController : Controller
    {
        private readonly HttpClient _httpClient;

        public ReviewController()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("http://localhost:5092/"); // API ka port check kar lena
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var token = Request.Cookies["JWToken"];
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Account");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var viewModel = new ReviewPageViewModel();

            try
            {
                var reviewResp = await _httpClient.GetAsync("api/Review/my-reviews");
                if (reviewResp.IsSuccessStatusCode)
                {
                    var json = await reviewResp.Content.ReadAsStringAsync();
                    var data = JsonSerializer.Deserialize<ReviewApiResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    viewModel.MyReviews = data?.data ?? new List<ReviewItemViewModel>();
                }

                var arenaResp = await _httpClient.GetAsync("api/Explore/arenas");
                if (arenaResp.IsSuccessStatusCode)
                {
                    var json = await arenaResp.Content.ReadAsStringAsync();
                    var arenaData = JsonSerializer.Deserialize<ReviewArenaResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    viewModel.Arenas = arenaData?.data ?? new List<ReviewArenaModel>();
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Connection Error: " + ex.Message;
            }

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Add(int arenaId, int rating, string comment)
        {
            var token = Request.Cookies["JWToken"];
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Account");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var reviewData = new { ArenaId = arenaId, Rating = rating, Comment = comment };
            var content = new StringContent(JsonSerializer.Serialize(reviewData), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("api/Review/add", content);

            if (response.IsSuccessStatusCode)
                TempData["Success"] = "Aapka Review successfully submit ho gaya!";
            else
                TempData["Error"] = "Review add karne mein koi problem aayi.";

            return RedirectToAction("Index");
        }

        // YAHAN HAIN NAYE EDIT AUR DELETE METHODS (Jinpe 405 error aa raha tha) 👇

        [HttpPost]
        public async Task<IActionResult> Edit(int reviewId, int arenaId, int rating, string comment)
        {
            var token = Request.Cookies["JWToken"];
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Account");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var reviewData = new { ArenaId = arenaId, Rating = rating, Comment = comment };
            var content = new StringContent(JsonSerializer.Serialize(reviewData), Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"api/Review/update/{reviewId}", content);

            if (response.IsSuccessStatusCode)
                TempData["Success"] = "Review successfully update ho gaya!";
            else
                TempData["Error"] = "Review update nahi ho paya.";

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int reviewId)
        {
            var token = Request.Cookies["JWToken"];
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Account");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.DeleteAsync($"api/Review/delete/{reviewId}");

            if (response.IsSuccessStatusCode)
                TempData["Success"] = "Review successfully delete ho gaya!";
            else
                TempData["Error"] = "Review delete nahi ho paya.";

            return RedirectToAction("Index");
        }
    }

    // --- VIEW MODELS ---
    public class ReviewPageViewModel
    {
        public List<ReviewItemViewModel>? MyReviews { get; set; }
        public List<ReviewArenaModel>? Arenas { get; set; }
    }

    public class ReviewApiResponse { public List<ReviewItemViewModel>? data { get; set; } }

    public class ReviewItemViewModel
    {
        public int reviewId { get; set; }
        public string? arenaName { get; set; }
        public int rating { get; set; }
        public string? comment { get; set; }
        public DateTime date { get; set; }
    }

    public class ReviewArenaResponse { public List<ReviewArenaModel>? data { get; set; } }

    public class ReviewArenaModel { public int arenaId { get; set; } public string? name { get; set; } }
}