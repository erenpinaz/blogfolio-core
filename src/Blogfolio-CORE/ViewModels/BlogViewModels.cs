using Blogfolio_CORE.Models;
using System;
using System.Collections.Generic;
using X.PagedList;

namespace Blogfolio_CORE.ViewModels
{
    public class PostItemModel
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public string Content { get; set; }
        public string Slug { get; set; }
        public bool CommentsEnabled { get; set; }
        public DateTime DateCreated { get; set; }

        public IEnumerable<PostCategory> PostCategories { get; set; }
    }

    public class CategoryItemModel
    {
        public string Name { get; set; }

        public string Slug { get; set; }

        public IPagedList<PostItemModel> Posts;
    }
}