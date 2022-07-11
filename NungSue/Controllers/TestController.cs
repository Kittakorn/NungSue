using Microsoft.AspNetCore.Mvc;

namespace NungSue.Controllers
{
    public class TestController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
