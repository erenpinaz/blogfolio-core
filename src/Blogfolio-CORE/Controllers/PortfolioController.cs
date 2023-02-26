using System.Linq;
using System.Threading.Tasks;
using Blogfolio_CORE.Data;
using Blogfolio_CORE.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Blogfolio_CORE.Controllers
{
    public class PortfolioController : BaseController
    {
        private readonly BlogfolioContext _context;

        public PortfolioController(BlogfolioContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? page = 1)
        {
            var projects = await _context.Projects.ToListAsync();

            var model = projects.Select(p => new ProjectItemModel()
            {
                Name = p.Name,
                Image = p.Image,
                Description = p.Description,
                Slug = p.Slug
            });

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Project(string slug)
        {
            var project = await _context.Projects.FirstOrDefaultAsync(q => q.Slug == slug);
            if (project == null)
            {
                return StatusCode(StatusCodes.Status404NotFound);
            }

            return PartialView("_Project", new ProjectItemModel()
            {
                Name = project.Name,
                Image = project.Image,
                Description = project.Description,
                Slug = project.Slug
            });
        }
    }
}