using Blogfolio_CORE.Areas.Admin.Identity;
using Blogfolio_CORE.Areas.Admin.ViewModels;
using Blogfolio_CORE.Filters;
using Blogfolio_CORE.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Blogfolio_CORE.Areas.Admin.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly IUserStore _userStore;

        public AccountController(IUserStore userStore)
        {
            _userStore = userStore;
        }

        #region Account

        [HttpGet]
        [AllowAnonymous]
        [ServiceFilter(typeof(CheckSetupFilter))]
        public IActionResult CreateAdmin()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [ServiceFilter(typeof(CheckSetupFilter))]
        public async Task<IActionResult> CreateAdmin(CreateAdminEditModel model)
        {
            if (ModelState.IsValid)
            {
                // Create the user
                if (!await _userStore.UserExistsAsync(model.UserName))
                {
                    var user = new User { UserName = model.UserName, Name = model.Name, Email = model.Email };
                    var result = await _userStore.CreateAsync(user, model.Password, "Administrator");
                    if (result.Success)
                    {
                        return RedirectToAction("Login", "Account", new { area = "Admin" });
                    }
                    ModelState.AddModelError(string.Empty, result.Error);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "UserName is already taken.");
                }
            }

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string returnUrl)
        {
            await _userStore.LogoutAsync(HttpContext);

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginEditModel model, string returnUrl)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                var result = await _userStore.LoginAsync(HttpContext, model.UserName, model.Password, model.RememberMe);
                if (result.Success)
                {
                    return RedirectToLocal(returnUrl);
                }
                ModelState.AddModelError(string.Empty, result.Error);
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _userStore.LogoutAsync(HttpContext);
            return RedirectToAction("Index", "Blog", new { area = "" });
        }

        #endregion

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Dashboard");
            }
        }
    }
}