using Blogfolio_CORE.Areas.Admin.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Memory;
using Blogfolio_CORE.Common.Services;
using System;

namespace Blogfolio_CORE.Controllers
{
    public class BaseController : Controller
    {
        private IMemoryCache MemoryCache => HttpContext?.RequestServices.GetService<IMemoryCache>();
        protected ISettingsService SettingsService => HttpContext?.RequestServices.GetService<ISettingsService>();

        protected SiteSettingsEditModel SiteSettings
        {
            get
            {
                MemoryCache.TryGetValue("SiteSettings", out SiteSettingsEditModel _siteSettings);
                if (_siteSettings != null)
                    return _siteSettings;

                _siteSettings = SettingsService.GetByName<SiteSettingsEditModel>("Settings/site-settings.json");
                MemoryCache.Set("SiteSettings", _siteSettings, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(3) });

                return _siteSettings;
            }
        }

        protected SocialSettingsEditModel SocialSettings
        {
            get
            {
                MemoryCache.TryGetValue("SocialSettings", out SocialSettingsEditModel _socialSettings);
                if (_socialSettings != null)
                    return _socialSettings;

                _socialSettings = SettingsService.GetByName<SocialSettingsEditModel>("Settings/social-settings.json");
                MemoryCache.Set("SocialSettings", _socialSettings, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(3) });

                return _socialSettings;
            }
        }

        public BaseController()
        {

        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            base.OnActionExecuted(filterContext);

            // Check if the view base and the model are present
            if (!(filterContext.Result is ViewResult)) return;

            // Set the base model and populate data
            ViewData["SiteSettings"] = SiteSettings;
            ViewData["SocialSettings"] = SocialSettings;
        }
    }
}