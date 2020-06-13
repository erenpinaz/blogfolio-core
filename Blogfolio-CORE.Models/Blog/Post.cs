using System;
using System.Collections.Generic;

namespace Blogfolio_CORE.Models
{
    public class Post
    {
        #region Fields

        private ICollection<PostCategory> _postCategories;
        private ICollection<Comment> _comments;

        #endregion

        #region Scalar Properties

        public Guid PostId { get; set; }
        public virtual Guid UserId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Summary { get; set; }
        public string Slug { get; set; }
        public bool CommentsEnabled { get; set; }
        public PostStatus Status { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateModified { get; set; }

        #endregion

        #region Navigation Properties

        public virtual ICollection<PostCategory> PostCategories
        {
            get { return _postCategories ?? (_postCategories = new List<PostCategory>()); }
            set { _postCategories = value; }
        }

        public virtual ICollection<Comment> Comments
        {
            get { return _comments ?? (_comments = new List<Comment>()); }
            set { _comments = value; }
        }

        public virtual User User { get; set; }

        #endregion
    }
}