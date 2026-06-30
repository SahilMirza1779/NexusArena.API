using Microsoft.AspNetCore.Mvc;

namespace NexusArena.Web.Controllers
{
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            // 🌟 THE FIX: Purana API call aur models sab delete maar diye!
            // Ab agar user login karke yahan aata hai, toh usko direct 
            // aapke naye Premium Timer wale Dashboard par fek do.
            return RedirectToAction("Index", "UserDashboard");
        }
    }
}