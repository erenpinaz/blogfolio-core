using Blogfolio_CORE.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace Blogfolio_CORE.Areas.Admin.ViewModels
{
    public class ProjectEditModel
    {
        public Guid? ProjectId { get; set; }

        [Required]
        [StringLength(64, MinimumLength = 3)]
        [DataType(DataType.Text)]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Image", Description = "Relative path of an image file")]
        public string Image { get; set; }

        [Required]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Required]
        [StringLength(32, MinimumLength = 3)]
        [DataType(DataType.Text)]
        [Display(Name = "Slug", Description = "URL-friendly version of the name")]
        public string Slug { get; set; }

        [Display(Name = "Status")]
        public ProjectStatus Status { get; set; }
    }
}
