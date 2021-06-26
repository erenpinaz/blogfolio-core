using AutoMapper;
using Blogfolio_CORE.Areas.Admin.ViewModels;
using Blogfolio_CORE.Data;
using Blogfolio_CORE.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Blogfolio_CORE.Areas.Admin.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class DashboardController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private readonly IMapper _mapper;

        private readonly BlogfolioContext _context;

        private const string MediaDirectory = "/images/media/";
        private const string ThumbDirectory = "/uploads/thumbs/";
        private const string UploadDirectory = "/uploads/";

        public DashboardController(IWebHostEnvironment env, IMapper mapper, BlogfolioContext context)
        {
            _env = env;
            _mapper = mapper;

            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        #region Blog

        #region Post

        [HttpGet]
        public IActionResult Posts()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> PostTableData(string order, string search, int offset, int limit)
        {
            var posts = await _context.Posts
                .Where(q => EF.Functions.Like(q.Title, $"%{search}%"))
                .Include(q => q.User)
                .Include(q => q.PostCategories).ThenInclude(qt => qt.Category)
                .Skip(offset).Take(limit)
                .ToListAsync();

            posts = order == "asc"
                ? posts.OrderBy(q => q.DateCreated).ToList()
                : posts.OrderByDescending(q => q.DateCreated).ToList();

            var json = new
            {
                total = posts.Count,
                rows = posts.Select(p => new
                {
                    postid = p.PostId,
                    title = p.Title,
                    summary = p.Summary,
                    author = p.User.Name,
                    categories = p.PostCategories.Select(c => new
                    {
                        name = c.Category.Name
                    }),
                    commentsenabled = p.CommentsEnabled,
                    status = p.Status.ToString(),
                    created = p.DateCreated,
                    updated = p.DateModified
                })
            };

            return new JsonResult(json);
        }

        [HttpGet]
        public IActionResult AddPost()
        {
            return View("PostRecord");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPost(PostEditModel model, Guid[] selectedCategories)
        {
            if (ModelState.IsValid)
            {
                if (selectedCategories != null && selectedCategories.Any())
                {
                    try
                    {
                        // Get the current user (No need to check if null because of the Authorize attribute)
                        var userId = Guid.Parse(User.Claims.FirstOrDefault(q => q.Type == ClaimTypes.NameIdentifier)?.Value);
                        var user = await _context.Users.FirstOrDefaultAsync(q => q.UserId == userId);

                        // Create post entity
                        var post = _mapper.Map<Post>(model);

                        var record = await _context.Posts.FirstOrDefaultAsync(q => q.PostId != model.PostId && q.Slug == model.Slug);
                        if (record == null)
                        {
                            // Populate unmapped properties
                            post.Slug = model.Slug;
                            post.User = user;
                            post.DateCreated = DateTime.UtcNow;

                            // Categorize
                            foreach (var categoryId in selectedCategories)
                            {
                                var category = await _context.Categories.FirstOrDefaultAsync(q => q.CategoryId == categoryId);
                                if (category != null)
                                {
                                    post.PostCategories.Add(new PostCategory { Post = post, Category = category });
                                }
                            }

                            // Add
                            _context.Posts.Add(post);
                            await _context.SaveChangesAsync();

                            return RedirectToAction("Posts", "Dashboard");
                        }
                        ModelState.AddModelError("slug", "Slug must be unique");
                    }
                    catch
                    {
                        ModelState.AddModelError("", "An error occurred while adding the post");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Category is required");
                }
            }

            return View("PostRecord", model);
        }

        [HttpGet]
        public async Task<IActionResult> UpdatePost(Guid? id)
        {
            var post = await _context.Posts.FirstOrDefaultAsync(q => q.PostId == id);
            if (post == null)
            {
                return StatusCode(StatusCodes.Status404NotFound);
            }
            return View("Postrecord", _mapper.Map<PostEditModel>(post));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdatePost(PostEditModel model, Guid[] selectedCategories)
        {
            if (ModelState.IsValid)
            {
                if (selectedCategories != null && selectedCategories.Any())
                {
                    try
                    {
                        // Get the post entity
                        var recPost = await _context.Posts.Include(q => q.User).Include(q => q.PostCategories).FirstOrDefaultAsync(q => q.PostId == model.PostId);

                        var record = await _context.Posts.FirstOrDefaultAsync(q => q.PostId != model.PostId && q.Slug == model.Slug);
                        if (record == null)
                        {
                            // Update unmapped properties
                            recPost.Title = model.Title;
                            recPost.Slug = model.Slug;
                            recPost.Summary = model.Summary;
                            recPost.Content = model.Content;
                            recPost.CommentsEnabled = model.CommentsEnabled;
                            recPost.Status = model.Status;
                            recPost.DateModified = DateTime.Now;

                            // Re categorize
                            recPost.PostCategories.Clear();
                            foreach (var categoryId in selectedCategories)
                            {
                                var category = await _context.Categories.FirstOrDefaultAsync(q => q.CategoryId == categoryId);
                                if (category != null)
                                {
                                    recPost.PostCategories.Add(new PostCategory { Post = recPost, Category = category });
                                }
                            }

                            // Update
                            _context.Posts.Update(recPost);
                            await _context.SaveChangesAsync();

                            return RedirectToAction("Posts", "Dashboard");
                        }
                        ModelState.AddModelError("slug", "Slug must be unique");
                    }
                    catch
                    {
                        ModelState.AddModelError("", "An error occurred while updating the post");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Category is required");
                }
            }

            return View("PostRecord", model);
        }

        [HttpGet]
        public async Task<IActionResult> PopulatePostCategories(Guid? id)
        {
            var categories = await _context.Categories.ToListAsync();
            var postCategoryIds = new HashSet<Guid>(await _context.PostCategories.Where(q => q.PostId == id).Select(q => q.CategoryId).ToListAsync());

            var json = categories.Select(c => new
            {
                id = c.CategoryId,
                name = c.Name,
                ischecked = postCategoryIds.Contains(c.CategoryId)
            });

            return new JsonResult(json);
        }

        [HttpPost]
        public async Task<IActionResult> DeletePost([FromBody]Guid? id)
        {
            try
            {
                var post = await _context.Posts.FirstOrDefaultAsync(q => q.PostId == id);
                if (post != null)
                {
                    _context.Posts.Remove(post);
                    await _context.SaveChangesAsync();
                }

                return StatusCode(StatusCodes.Status200OK);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost]
        public async Task<IActionResult> BulkDeletePost([FromBody]Guid[] ids)
        {
            try
            {
                foreach (var id in ids)
                {
                    var post = await _context.Posts.FirstOrDefaultAsync(q => q.PostId == id);
                    if (post != null)
                    {
                        _context.Posts.Remove(post);
                    }
                }
                await _context.SaveChangesAsync();

                return StatusCode(StatusCodes.Status200OK);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        #endregion

        #region Category

        [HttpGet]
        public IActionResult Categories()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> CategoryTableData(string order, string search, int offset, int limit)
        {
            var categories = await _context.Categories
                .Include(qt => qt.PostCategories)
                .Where(q => EF.Functions.Like(q.Name, $"%{search}%"))
                .Skip(offset).Take(limit)
                .ToListAsync();

            categories = order == "asc"
                ? categories.OrderBy(q => q.DateCreated).ToList()
                : categories.OrderByDescending(q => q.DateCreated).ToList();

            var json = new
            {
                total = categories.Count,
                rows = categories.Select(c => new
                {
                    categoryid = c.CategoryId,
                    name = c.Name,
                    slug = c.Slug,
                    totalposts = c.PostCategories.Count,
                    created = c.DateCreated,
                    updated = c.DateModified
                })
            };

            return new JsonResult(json);
        }

        [HttpGet]
        public IActionResult AddCategory()
        {
            return PartialView("_CategoryRecord");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCategory(CategoryEditModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var category = _mapper.Map<Category>(model);

                    var record = await _context.Categories.FirstOrDefaultAsync(q => q.CategoryId != model.CategoryId && q.Slug == model.Slug);
                    if (record == null)
                    {
                        // Populate unmapped properties
                        category.Slug = model.Slug;
                        category.DateCreated = DateTime.UtcNow;

                        // Add
                        _context.Categories.Add(category);
                        await _context.SaveChangesAsync();

                        return Json(new { success = true });
                    }
                    ModelState.AddModelError("slug", "Slug must be unique");
                }
                catch
                {
                    ModelState.AddModelError("", "An error occurred while adding the category");
                }
            }
            return PartialView("_CategoryRecord", model);
        }

        [HttpGet]
        public async Task<IActionResult> UpdateCategory(Guid? id)
        {
            var category = await _context.Categories.FirstOrDefaultAsync(q => q.CategoryId == id);
            if (category == null)
            {
                return StatusCode(StatusCodes.Status404NotFound);
            }
            return PartialView("_CategoryRecord", _mapper.Map<CategoryEditModel>(category));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCategory(CategoryEditModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var recCategory = await _context.Categories.FirstOrDefaultAsync(q => q.CategoryId == model.CategoryId);

                    // Get the category entity
                    var record = await _context.Categories.FirstOrDefaultAsync(q => q.CategoryId != model.CategoryId && q.Slug == model.Slug);
                    if (record == null)
                    {
                        // Update unmapped properties
                        recCategory.Name = model.Name;
                        recCategory.Slug = model.Slug;
                        recCategory.DateModified = DateTime.UtcNow;

                        // Update
                        _context.Categories.Update(recCategory);
                        await _context.SaveChangesAsync();

                        return Json(new { success = true });
                    }
                    ModelState.AddModelError("slug", "Slug must be unique");
                }
                catch
                {
                    ModelState.AddModelError("", "An error occurred while updating the category");
                }
            }

            return PartialView("_CategoryRecord", model);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCategory([FromBody]Guid? id)
        {
            try
            {
                var category = await _context.Categories.FirstOrDefaultAsync(q => q.CategoryId == id);
                if (category != null)
                {
                    _context.Categories.Remove(category);
                    await _context.SaveChangesAsync();
                }

                return StatusCode(StatusCodes.Status200OK);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost]
        public async Task<ActionResult> BulkDeleteCategory([FromBody]Guid[] ids)
        {
            try
            {
                foreach (var id in ids)
                {
                    var category = await _context.Categories.FirstOrDefaultAsync(q => q.CategoryId == id);
                    if (category != null)
                    {
                        _context.Categories.Remove(category);
                    }
                }
                await _context.SaveChangesAsync();

                return StatusCode(StatusCodes.Status200OK);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
        #endregion

        #endregion

        #region Portfolio

        [HttpGet]
        public IActionResult Projects()
        {
            return View();
        }

        [HttpGet]
        public IActionResult AddProject()
        {
            return View("ProjectRecord", new ProjectEditModel { Image = "/images/placeholder.png" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProject(ProjectEditModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Create post entity
                    var project = _mapper.Map<Project>(model);

                    var record = await _context.Projects.FirstOrDefaultAsync(q => q.ProjectId != model.ProjectId && q.Slug == model.Slug);
                    if (record == null)
                    {
                        // Populate unmapped properties
                        project.Slug = model.Slug;
                        project.DateCreated = DateTime.UtcNow;

                        // Add
                        _context.Projects.Add(project);
                        await _context.SaveChangesAsync();

                        return RedirectToAction("Projects", "Dashboard");
                    }
                    ModelState.AddModelError("slug", "Slug must be unique");
                }
                catch
                {
                    ModelState.AddModelError("", "An error occurred while adding the post");
                }
            }

            return View("ProjectRecord", model);
        }

        [HttpGet]
        public async Task<IActionResult> UpdateProject(Guid? id)
        {
            var project = await _context.Projects.FirstOrDefaultAsync(q => q.ProjectId == id);
            if (project == null)
            {
                return StatusCode(StatusCodes.Status404NotFound);
            }
            return View("ProjectRecord", _mapper.Map<ProjectEditModel>(project));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProject(ProjectEditModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Get the project entity
                    var recProject = await _context.Projects.FirstOrDefaultAsync(q => q.ProjectId == model.ProjectId);

                    var record = await _context.Projects.FirstOrDefaultAsync(q => q.ProjectId != model.ProjectId && q.Slug == model.Slug);
                    if (record == null)
                    {
                        // Update unmapped properties
                        recProject.Name = model.Name;
                        recProject.Slug = model.Slug;
                        recProject.Description = model.Description;
                        recProject.Image = model.Image;
                        recProject.Status = model.Status;
                        recProject.DateModified = DateTime.UtcNow;

                        // Update
                        _context.Projects.Update(recProject);
                        await _context.SaveChangesAsync();

                        return RedirectToAction("Projects", "Dashboard");
                    }
                    ModelState.AddModelError("slug", "Slug must be unique.");
                }
                catch
                {
                    ModelState.AddModelError("", "An error occurred while updating the project.");
                }
            }

            return View("ProjectRecord", model);
        }

        [HttpPost]
        public async Task<ActionResult> DeleteProject([FromBody]Guid? id)
        {
            try
            {
                var project = await _context.Projects.FirstOrDefaultAsync(q => q.ProjectId == id);
                if (project != null)
                {
                    _context.Projects.Remove(project);
                    await _context.SaveChangesAsync();
                }

                return StatusCode(StatusCodes.Status200OK);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost]
        public async Task<ActionResult> BulkDeleteProject([FromBody]Guid[] ids)
        {
            try
            {
                foreach (var id in ids)
                {
                    var project = await _context.Projects.FirstOrDefaultAsync(q => q.ProjectId == id);
                    if (project != null)
                    {
                        _context.Projects.Remove(project);
                    }
                }
                await _context.SaveChangesAsync();

                return StatusCode(StatusCodes.Status200OK);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet]
        public async Task<IActionResult> ProjectTableData(string order, string search, int offset, int limit)
        {
            var projects = await _context.Projects
                .Where(q => EF.Functions.Like(q.Name, $"%{search}%"))
                .Skip(offset).Take(limit)
                .ToListAsync();

            projects = order == "asc"
                ? projects.OrderBy(q => q.DateCreated).ToList()
                : projects.OrderByDescending(q => q.DateCreated).ToList();

            var json = new
            {
                total = projects.Count,
                rows = projects.Select(p => new
                {
                    projectid = p.ProjectId,
                    name = p.Name,
                    image = p.Image,
                    description = p.Description,
                    status = p.Status.ToString(),
                    created = p.DateCreated,
                    updated = p.DateModified
                })
            };

            return new JsonResult(json);
        }

        #endregion

        #region Media

        [HttpGet]
        public IActionResult Media()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> MediaTableData(string order, string search, int offset, int limit)
        {
            var total = await _context.Medias.CountAsync();

            var media = await _context.Medias
                .Where(q => EF.Functions.Like(q.Name, $"%{search}%"))
                .Skip(offset).Take(limit)
                .ToListAsync();

            media = order == "asc"
                ? media.OrderBy(q => q.DateCreated).ToList()
                : media.OrderByDescending(q => q.DateCreated).ToList();

            var json = new
            {
                total = total,
                rows = media.Select(m => new
                {
                    mediaid = m.MediaId,
                    name = m.Name,
                    path = m.Path,
                    thumbpath = m.ThumbPath,
                    size = m.Size,
                    type = m.Type,
                    created = m.DateCreated
                })
            };

            return new JsonResult(json);
        }

        [HttpPost]
        public async Task<IActionResult> UploadMedia()
        {
            try
            {
                var file = Request.Form.Files[0];
                if (file != null)
                {
                    var fileName = GenerateUniqueFileName(file.FileName);

                    var uploadPath = Path.Combine(UploadDirectory, fileName);
                    var thumbPath = GetThumbnail(file.ContentType, fileName);

                    // Create media entity
                    var media = new Media()
                    {
                        MediaId = Guid.NewGuid(),
                        Name = file.FileName,
                        Path = uploadPath,
                        ThumbPath = thumbPath,
                        Type = file.ContentType,
                        Size = file.Length.ToString(),
                        DateCreated = DateTime.UtcNow
                    };

                    // Save media file to disk
                    var saveMedia = await SaveFile(file, media);
                    if (!saveMedia.Success)
                    {
                        return Json(saveMedia.Error);
                    }

                    _context.Medias.Add(media);
                    await _context.SaveChangesAsync();

                    return Json(new
                    {
                        success = true,
                        id = media.MediaId,
                        path = media.Path,
                        thumbpath = media.ThumbPath,
                        size = media.Size,
                        type = media.Type,
                        created = media.DateCreated
                    });
                }
                return Json(new { success = false });
            }
            catch
            {
                return Json(new { success = false });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteMedia([FromBody]Guid? id)
        {
            try
            {
                var media = await _context.Medias.FirstOrDefaultAsync(q => q.MediaId == id);
                if (media != null)
                {
                    await DeleteFileFromDiskAsync(media.Path);

                    if (!media.ThumbPath.StartsWith(MediaDirectory))
                    {
                        await DeleteFileFromDiskAsync(media.ThumbPath);
                    }

                    _context.Medias.Remove(media);
                    await _context.SaveChangesAsync();
                }

                return StatusCode(StatusCodes.Status200OK);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost]
        public async Task<IActionResult> BulkDeleteMedia([FromBody]Guid[] ids)
        {
            try
            {
                foreach (var id in ids)
                {
                    var media = await _context.Medias.FirstOrDefaultAsync(q => q.MediaId == id);
                    if (media != null)
                    {
                        await DeleteFileFromDiskAsync(media.Path);

                        if (!media.ThumbPath.StartsWith(MediaDirectory))
                        {
                            await DeleteFileFromDiskAsync(media.ThumbPath);
                        }

                        _context.Medias.Remove(media);
                    }
                }
                await _context.SaveChangesAsync();

                return StatusCode(StatusCodes.Status200OK);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet]
        public async Task<ActionResult> GetFilesByType(string type)
        {
            var media = await _context.Medias.Where(q => q.Type.StartsWith(type)).ToListAsync();
            var json = media.OrderByDescending(i => i.DateCreated).Select(i => new
            {
                mediaid = i.MediaId,
                name = i.Name,
                path = i.Path,
                thumbpath = i.ThumbPath,
                type = i.Type
            });

            return new JsonResult(json);
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Checks and creates server directories
        /// </summary>
        /// <param name="virtualPaths"></param>
        private static void CheckCreateDirectories(string[] virtualPaths)
        {
            foreach (var path in virtualPaths)
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
        }

        /// <summary>
        /// Asynchronously saves given file to disk
        /// </summary>
        /// <param name="file"></param>
        /// <param name="media"></param>
        /// <returns>A <see cref="bool"/></returns>
        private async Task<UploadResult> SaveFile(IFormFile file, Media media)
        {
            try
            {
                if (file == null)
                    throw new ArgumentNullException(nameof(file));

                if (media == null)
                    throw new ArgumentNullException(nameof(media));

                // Create directories
                CheckCreateDirectories(new string[] {
                    Path.Combine(_env.WebRootPath, UploadDirectory.TrimStart('/')),
                    Path.Combine(_env.WebRootPath, ThumbDirectory.TrimStart('/'))
                });

                // Save with a thumbnail if the file is an image file
                if (file.ContentType.StartsWith("image"))
                {
                    using (var stream = file.OpenReadStream())
                    {
                        SaveImage(stream, Path.Combine(_env.WebRootPath, media.Path.TrimStart('/')), 1280, InterpolationMode.HighQualityBicubic);
                        SaveImage(stream, Path.Combine(_env.WebRootPath, media.ThumbPath.TrimStart('/')), 300, InterpolationMode.Low);
                    }
                }
                else
                {
                    using (var bits = new FileStream(Path.Combine(_env.WebRootPath, media.Path.TrimStart('/')), FileMode.Create))
                    {
                        await file.CopyToAsync(bits);
                    }
                }

                return new UploadResult(true);
            }
            catch (Exception e)
            {
                return new UploadResult(false, e.Message);
            }
        }

        /// <summary>
        /// Creates a resized bitmap from the given stream. Resizes the image by 
        /// creating an aspect ratio safe image. Image is sized to the larger size of width
        /// height and then smaller size is adjusted by aspect ratio.
        /// 
        /// Credits: https://github.com/RickStrahl/Westwind.plUploadHandler
        /// </summary>
        /// <param name="fileStream"></param>
        /// <param name="outputFilename"></param>
        /// <param name="height"></param>
        private static void SaveImage(Stream fileStream, string outputFilename, int height, InterpolationMode interpolationMode)
        {
            Bitmap bmp = null;
            Bitmap bmpOut = null;
            Graphics g = null;

            try
            {
                bmp = new Bitmap(fileStream);
                var format = bmp.RawFormat;

                var newWidth = 0;
                var newHeight = 0;

                if (bmp.Height < height)
                {
                    bmp.Save(outputFilename);
                }
                else
                {
                    var ratio = (decimal)height / bmp.Height;
                    newHeight = height;
                    newWidth = Convert.ToInt32(bmp.Width * ratio);

                    bmpOut = new Bitmap(newWidth, newHeight);
                    g = Graphics.FromImage(bmpOut);
                    g.InterpolationMode = interpolationMode;
                    g.FillRectangle(Brushes.White, 0, 0, newWidth, newHeight);
                    g.DrawImage(bmp, 0, 0, newWidth, newHeight);

                    bmpOut.Save(outputFilename, format);
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                bmp?.Dispose();
                bmpOut?.Dispose();
                g?.Dispose();
            }
        }

        /// <summary>
        /// Generates a unique name for a file
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>A <see cref="string"/></returns>
        private static string GenerateUniqueFileName(string fileName)
        {
            return string.Format("UL_{0}{1}", Guid.NewGuid().ToString("N").ToUpper(),
                Path.GetExtension(fileName));
        }

        /// <summary>
        /// Asynchronously deletes a directory from the disk
        /// </summary>
        /// <param name="directory"></param>
        /// <returns>A <see cref="bool"/></returns>
        private async Task<bool> DeleteDirectoryFromDiskAsync(string directory)
        {
            try
            {
                if (directory == null)
                    throw new ArgumentNullException(nameof(directory));

                var serverDirectory = Path.Combine(_env.WebRootPath, directory);
                if (Directory.Exists(serverDirectory))
                {
                    await Task.Factory.StartNew(() => Directory.Delete(serverDirectory, true));
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Asynchronously deletes a file from the disk
        /// </summary>
        /// <param name="file"></param>
        /// <returns>A <see cref="bool" /></returns>
        private async Task<bool> DeleteFileFromDiskAsync(string file)
        {
            try
            {
                if (file == null)
                    throw new ArgumentNullException(nameof(file));

                var serverFile = Path.Combine(_env.WebRootPath, file);
                if (System.IO.File.Exists(serverFile))
                {
                    var fInfo = new FileInfo(serverFile);
                    await Task.Factory.StartNew(() => fInfo.Delete());
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Returns thumbnail path for an image
        /// Returns default thumbnail paths for known mime types
        /// </summary>
        /// <param name="mimeType"></param>
        /// <param name="fileName"></param>
        private string GetThumbnail(string mimeType, string fileName)
        {
            if (fileName == null)
                throw new ArgumentNullException(nameof(fileName));

            if (mimeType == null)
                throw new ArgumentNullException(nameof(mimeType));

            // Default file thumbnail
            var thumbnail = Path.Combine(MediaDirectory, "default.png");

            if (mimeType.StartsWith("image")) // Images
            {
                thumbnail = Path.Combine(ThumbDirectory, fileName);
            }
            if (mimeType == ("application/zip") || mimeType == ("application/x-rar-compressed") ||
                mimeType == ("application/octet-stream")) // Archives
            {
                thumbnail = Path.Combine(MediaDirectory, "archive.png");
            }
            if (mimeType.StartsWith("text")) // Documents
            {
                thumbnail = Path.Combine(MediaDirectory, "text.png");
            }
            if (mimeType.StartsWith("audio")) // Audio files
            {
                thumbnail = Path.Combine(MediaDirectory, "audio.png");
            }
            if (mimeType.StartsWith("video")) // Video files
            {
                thumbnail = Path.Combine(MediaDirectory, "video.png");
            }

            return thumbnail;
        }

        public class UploadResult
        {
            public bool Success { get; set; }

            public string Error { get; set; }

            public UploadResult(bool success)
            {
                this.Success = success;
            }

            public UploadResult(bool success, string error)
            {
                this.Success = success;
                this.Error = error;
            }
        }

        #endregion
    }
}