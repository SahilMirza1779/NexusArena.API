using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NexusArena.Web.Controllers
{
    public class LandingController : Controller
    {
        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult OwnerApplication()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SubmitApplication()
        {
            return Content("Application Submitted Successfully! Our Admin will review your turf.");
        }
    }
}