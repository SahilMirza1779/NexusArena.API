using Microsoft.AspNetCore.Mvc;

namespace NexusArena.Web.Controllers
{
    public class OwnerDashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
