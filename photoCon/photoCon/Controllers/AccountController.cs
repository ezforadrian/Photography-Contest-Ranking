using Microsoft.AspNetCore.Mvc;

namespace photoCon.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
