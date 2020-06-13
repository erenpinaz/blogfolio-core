using AutoMapper;
using Blogfolio_CORE.Areas.Admin.Identity;
using Blogfolio_CORE.Common.SEO.Sitemap;
using Blogfolio_CORE.Common.Services;
using Blogfolio_CORE.Common.Services.Captcha;
using Blogfolio_CORE.Common.Services.Email;
using Blogfolio_CORE.Data;
using Blogfolio_CORE.Filters;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;

namespace Blogfolio_CORE
{
    public class Startup
    {
        public IConfiguration Configuration { get; private set; }
        public static readonly ILoggerFactory ConsoleLoggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .AddFilter((category, level) => category == DbLoggerCategory.Database.Command.Name && level == LogLevel.Information)
                .AddConsole();
        });

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            // Memory Cache
            services.AddMemoryCache();

            // Use MVC
            services.AddControllersWithViews()
                .AddRazorRuntimeCompilation();

            // PostgreSQL Configuration
            services.AddDbContext<BlogfolioContext>(options =>
            {
                options.UseNpgsql(this.Configuration.GetConnectionString("BlogfolioContext"), npopts => npopts.MigrationsAssembly(typeof(BlogfolioContext).Assembly.FullName));
                options.UseLoggerFactory(ConsoleLoggerFactory);
            });

            // Authentication
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Admin/Account/Login";
                    options.LogoutPath = "/";
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
                    options.SlidingExpiration = false;
                });

            // DTO - Entity Mapper
            services.AddAutoMapper(typeof(Startup));

            // DI Registers
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<ISettingsService, JsonSettingsService>();
            services.AddSingleton<IEmailService, EmailService>();
            services.AddSingleton<ICaptchaService, ReCaptchaService>();
            services.AddSingleton<ISitemapGenerator, SitemapGenerator>();
            services.AddScoped<IUserStore, UserStore>();
            services.AddScoped<CheckSetupFilter>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ISettingsService settingsService, IMemoryCache cache)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapAreaControllerRoute(
                    name: "admin",
                    areaName: "Admin",
                    pattern: "Admin/{controller=Dashboard}/{action=Index}/{id?}");

                endpoints.MapControllerRoute(
                    name: "project",
                    pattern: "portfolio/{slug}/",
                    defaults: new { controller = "Portfolio", action = "Project" });

                endpoints.MapControllerRoute(
                    name: "category-paged-first",
                    pattern: "blog/category/{slug}/",
                    defaults: new { controller = "Blog", action = "Category", page = 1 });

                endpoints.MapControllerRoute(
                    name: "category-paged",
                    pattern: "blog/category/{slug}/page/{page}/",
                    defaults: new { controller = "Blog", action = "Category" },
                    constraints: new { page = @"\d+" });

                endpoints.MapControllerRoute(
                    name: "post",
                    pattern: "blog/post/{year}/{month}/{slug}",
                    defaults: new { controller = "Blog", action = "Post" },
                    constraints: new { year = @"\d+", month = @"\d+" });

                endpoints.MapControllerRoute(
                    name: "paged-first",
                    pattern: "/",
                    defaults: new { controller = "Blog", action = "Index", page = 1 });

                endpoints.MapControllerRoute(
                    name: "paged",
                    pattern: "page/{page=1}",
                    defaults: new { controller = "Blog", action = "Index" });

                endpoints.MapControllerRoute(
                    name: "Sitemap",
                    pattern: "sitemap.xml",
                    defaults: new { controller = "Sitemap", action = "Index" });

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Blog}/{action=Index}");
            });
        }
    }
}
