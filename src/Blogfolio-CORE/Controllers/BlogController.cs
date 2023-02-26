using Blogfolio_CORE.Data;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Blogfolio_CORE.ViewModels;
using X.PagedList;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ServiceModel.Syndication;
using System;
using System.IO;
using System.Xml;
using System.Text;
using Blogfolio_CORE.Models;

namespace Blogfolio_CORE.Controllers
{
    public class BlogController : BaseController
    {
        private readonly BlogfolioContext _context;

        public BlogController(BlogfolioContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? page = 1)
        {
            var posts = await _context.Posts
                .Where(q => q.Status != PostStatus.Draft)
                .Include(p => p.User)
                .Include(p => p.PostCategories).ThenInclude(pc => pc.Category)
                .ToListAsync();

            var model = posts.Select(p => new PostItemModel()
            {
                Title = p.Title,
                Author = p.User.Name,
                Content = p.Content,
                Slug = p.Slug,
                CommentsEnabled = p.CommentsEnabled,
                DateCreated = p.DateCreated,
                PostCategories = p.PostCategories.ToList()
            }).ToPagedList(page ?? 1, SiteSettings.PageSize);

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Post(int year, int month, string slug)
        {
            var post = await _context.Posts
                .Include(q => q.User)
                .Include(q => q.PostCategories).ThenInclude(q => q.Category)
                .FirstOrDefaultAsync(q => q.DateCreated.Year == year && q.DateCreated.Month == month && q.Slug == slug);

            if (post == null)
            {
                return StatusCode(StatusCodes.Status404NotFound);
            }

            return View(new PostItemModel()
            {
                Title = post.Title,
                Author = post.User.Name,
                Slug = post.Slug,
                Content = post.Content,
                CommentsEnabled = post.CommentsEnabled,
                DateCreated = post.DateCreated,
                PostCategories = post.PostCategories
            });
        }

        [HttpGet]
        public async Task<IActionResult> Category(string slug, int? page)
        {
            var category = await _context.Categories
                .Include(q => q.PostCategories).ThenInclude(q => q.Post).ThenInclude(q => q.User)
                .FirstOrDefaultAsync(q => q.Slug == slug);

            if (category == null)
            {
                return StatusCode(StatusCodes.Status404NotFound);
            }

            return View(new CategoryItemModel()
            {
                Name = category.Name,
                Slug = category.Slug,
                Posts = category.PostCategories
                    .Where(q => q.Post.Status != PostStatus.Draft)
                    .Select(q => new PostItemModel()
                    {
                        Title = q.Post.Title,
                        Author = q.Post.User.Name,
                        Content = q.Post.Summary,
                        Slug = q.Post.Slug,
                        CommentsEnabled = q.Post.CommentsEnabled,
                        DateCreated = q.Post.DateCreated
                    }).ToPagedList(page ?? 1, SiteSettings.PageSize)
            });
        }

        [HttpGet]
        public async Task<IActionResult> Feed()
        {
            // Get posts
            var posts = await _context.Posts
                .Include(q => q.User)
                .Include(q => q.PostCategories).ThenInclude(q => q.Category)
                .ToListAsync();

            // Create feed items
            var feedItems = new List<SyndicationItem>();
            foreach (var post in posts.Take(SiteSettings.FeedSize))
            {
                var feedItem = new SyndicationItem
                {
                    Title = SyndicationContent.CreatePlaintextContent(post.Title),
                    Summary = SyndicationContent.CreatePlaintextContent(post.Summary),
                    Content = SyndicationContent.CreateHtmlContent(post.Content),
                    Authors = { new SyndicationPerson(post.User.Name) },
                    Links =
                    {
                        SyndicationLink.CreateAlternateLink(new Uri(Url.Action("Post", "Blog", new
                        {
                            year = post.DateCreated.Year,
                            month = post.DateCreated.Month,
                            slug = post.Slug
                        }, HttpContext.Request.Scheme)))
                    },
                    PublishDate = post.DateCreated
                };
                foreach (var postCategory in post.PostCategories)
                {
                    feedItem.Categories.Add(new SyndicationCategory(postCategory.Category.Name));
                }

                feedItems.Add(feedItem);
            }

            // Create feed
            var feed = new SyndicationFeed
            {
                Title = SyndicationContent.CreatePlaintextContent(SiteSettings.Title),
                Description = SyndicationContent.CreatePlaintextContent(SiteSettings.Tagline),
                Links = { SyndicationLink.CreateAlternateLink(new Uri(Url.Action("Index", "Blog", null, HttpContext.Request.Scheme))) },
                Items = feedItems
            };

            // Serve the feed
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
                    var rssFormatter = new Rss20FeedFormatter(feed, false);
                    rssFormatter.WriteTo(xmlWriter);
                    xmlWriter.Flush();
                }
                return File(stream.ToArray(), "application/rss+xml; charset=utf-8");
            }
        }
    }
}