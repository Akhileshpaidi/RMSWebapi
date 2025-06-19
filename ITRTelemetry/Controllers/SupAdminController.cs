using Microsoft.AspNetCore.Mvc;

namespace ITRTelemetry.Controllers
{
    public class SupAdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
