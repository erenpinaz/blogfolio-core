using System;
using System.Collections.Generic;

namespace Blogfolio_CORE.Models
{
    public class Role
    {
        #region Fields

        private ICollection<UserRole> _userRoles;

        #endregion

        #region Scalar Properties

        public Guid RoleId { get; set; }
        public string Name { get; set; }

        #endregion

        #region Navigation Properties

        public ICollection<UserRole> UserRoles
        {
            get { return _userRoles ?? (_userRoles = new List<UserRole>()); }
            set { _userRoles = value; }
        }

        #endregion
    }
}