using System;

namespace Blogfolio_CORE.Models
{
    public class PostCategory
    {
        #region Fields

        private Post _post;
        private Category _category;

        #endregion

        #region Scalar Properties

        public Guid CategoryId { get; set; }
        public Guid PostId { get; set; }

        #endregion

        #region Navigation Properties

        public virtual Post Post
        {
            get { return _post ?? (_post = new Post()); }
            set { _post = value; }
        }

        public virtual Category Category
        {
            get { return _category ?? (_category = new Category()); }
            set { _category = value; }
        }

        #endregion
    }
}