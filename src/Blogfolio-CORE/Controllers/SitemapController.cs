using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Blogfolio_CORE.Common.SEO.Sitemap;
using Blogfolio_CORE.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Blogfolio_CORE.Controllers
{
    public class SitemapController : Controller
    {
        private readonly BlogfolioContext _context;
        private readonly ISitemapGenerator _generator;

        public SitemapController(BlogfolioContext context, ISitemapGenerator generator)
        {
            _context = context;
            _generator = generator;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Add main sections 
            var sitemapItems = new List<SitemapItem>
            {
                new SitemapItem(Url.Action("Index", "Blog", null, HttpContext.Request.Scheme), changeFrequency: SitemapChangeFrequency.Weekly,
                    priority: 1.0),
                new SitemapItem(Url.Action("Index", "Portfolio", null, HttpContext.Request.Scheme),
                    changeFrequency: SitemapChangeFrequency.Monthly,
                    priority: 1.0),
                new SitemapItem(Url.Action("Index", "About", null, HttpContext.Request.Scheme), changeFrequency: SitemapChangeFrequency.Yearly,
                    priority: 0.4),
                new SitemapItem(Url.Action("Index", "Contact", null, HttpContext.Request.Scheme), changeFrequency: SitemapChangeFrequency.Yearly,
                    priority: 0.4)
            };

            // Add posts 
            var posts = await _context.Posts.ToListAsync();
            sitemapItems.AddRange(posts.Select(post => new SitemapItem(Url.Action("Post", "Blog", new
            {
                year = post.DateCreated.Year,
                month = post.DateCreated.Month,
                slug = post.Slug
            }, HttpContext.Request.Scheme), changeFrequency: SitemapChangeFrequency.Weekly, priority: 0.9)));

            // Add categories 
            var categories = await _context.Categories.ToListAsync();
            sitemapItems.AddRange(
                categories.Select(category => new SitemapItem(Url.Action("Category", "Blog", new
                {
                    slug = category.Slug
                }, HttpContext.Request.Scheme), changeFrequency: SitemapChangeFrequency.Weekly, priority: 0.7)));

            // Add projects 
            var projects = await _context.Projects.ToListAsync();
            sitemapItems.AddRange(
                projects.Select(project => new SitemapItem(Url.Action("Project", "Portfolio", new
                {
                    slug = project.Slug
                }, HttpContext.Request.Scheme), changeFrequency: SitemapChangeFrequency.Yearly, priority: 0.8)));

            // Serve the sitemap
            var settings = new XmlWriterSettings
            {
                Encoding = Encoding.UTF8,
                NewLineHandling = NewLineHandling.Entitize,
                NewLineOnAttributes = true,
                Indent = true
            };
            using (var stream = new MemoryStream())
            {
                using (var xmlWriter = XmlWriter.Create(stream, settings))
                {
                    var sitemap = _generator.GenerateSiteMap(sitemapItems);
                    sitemap.WriteTo(xmlWriter);

                    xmlWriter.Flush();
                }
                return File(stream.ToArray(), "text/xml; charset=utf-8");
            }
        }
    }
}