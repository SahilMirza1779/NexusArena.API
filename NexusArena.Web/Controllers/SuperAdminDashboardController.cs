using Microsoft.AspNetCore.Mvc;

namespace NexusArena.Web.Controllers
{
    public class SuperAdminDashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
