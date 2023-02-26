using Blogfolio_CORE.Areas.Admin.ViewModels;
using Blogfolio_CORE.Common.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace Blogfolio_CORE.Areas.Admin.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class SettingsController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private readonly ISettingsService _settingsService;

        public SettingsController(IWebHostEnvironment env, ISettingsService settingsService)
        {
            _settingsService = settingsService;
            _env = env;
        }

        [HttpGet]
        public IActionResult General(string message)
        {
            ViewData["StatusMessage"] = message;

            var filePath = Path.Combine(_env.ContentRootPath, "Settings", "site-settings.json");
            var model = _settingsService.GetByName<SiteSettingsEditModel>(filePath);

            return View(model);
        }

        [HttpPost, ActionName("General")]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateGeneral(SiteSettingsEditModel model)
        {
            if (ModelState.IsValid)
            {
                var filePath = Path.Combine(_env.ContentRootPath, "Settings", "site-settings.json");
                _settingsService.Save(model, filePath);
            }

            return RedirectToAction("General", "Settings", new { area = "Admin", message = "Settings saved successfully." });
        }

        [HttpGet]
        public IActionResult Social(string message)
        {
            ViewData["StatusMessage"] = message;

            var filePath = Path.Combine(_env.ContentRootPath, "Settings", "social-settings.json");
            var model = _settingsService.GetByName<SocialSettingsEditModel>(filePath);

            return View(model);
        }

        [HttpPost, ActionName("Social")]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateSocial(SocialSettingsEditModel model)
        {
            if (ModelState.IsValid)
            {
                var filePath = Path.Combine(_env.ContentRootPath, "Settings", "social-settings.json");
                _settingsService.Save(model, filePath);
            }

            return RedirectToAction("Social", "Settings", new { area = "Admin", message = "Settings saved successfully." });
        }

        [HttpGet]
        public IActionResult AddSocialItem()
        {
            return PartialView("_SocialItem", new SocialItemEditModel());
        }
    }
}