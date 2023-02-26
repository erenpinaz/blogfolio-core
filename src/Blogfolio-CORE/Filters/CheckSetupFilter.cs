using Blogfolio_CORE.Areas.Admin.ViewModels;
using Blogfolio_CORE.Common.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.IO;
using System.Net;

namespace Blogfolio_CORE.Filters
{
    /// <summary>
    /// Checks the setup status value from the site  settings
    /// file and prevents access to the main app if it is in 
    /// progress (value: 0)
    /// </summary>
    public class CheckSetupFilter : IActionFilter
    {
        private readonly IWebHostEnvironment _env;
        private readonly ISettingsService _settingsService;

        public CheckSetupFilter(IWebHostEnvironment env, ISettingsService settingsService)
        {
            _env = env;
            _settingsService = settingsService;
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {

        }

        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var siteSettings = _settingsService.GetByName<SiteSettingsEditModel>(Path.Combine(_env.ContentRootPath, "Settings", "site-settings.json"));
            var currentAction = (string)filterContext.RouteData.Values["action"];

            if (siteSettings.SetupCompleted)
            {
                if (currentAction.Equals("CreateAdmin", StringComparison.InvariantCultureIgnoreCase))
                {
                    filterContext.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
                }
            }
        }
    }
}
