using Microsoft.AspNetCore.Mvc;

namespace Blogfolio_CORE.Controllers
{
    public class AboutController : BaseController
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}