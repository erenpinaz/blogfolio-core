using System;
using System.Collections.Generic;

namespace Blogfolio_CORE.Models
{
    public class Category
    {
        #region Fields

        private ICollection<PostCategory> _postCategories;

        #endregion

        #region Scalar Properties

        public Guid CategoryId { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateModified { get; set; }

        #endregion

        #region Navigation Properties

        public virtual ICollection<PostCategory> PostCategories
        {
            get { return _postCategories ?? (_postCategories = new List<PostCategory>()); }
            set { _postCategories = value; }
        }

        #endregion
    }
}