using System;

namespace Blogfolio_CORE.Models
{
    public class UserRole
    {
        #region Fields

        private User _user;
        private Role _role;

        #endregion

        #region Scalar Properties

        public Guid RoleId { get; set; }
        public Guid UserId { get; set; }

        #endregion

        #region Navigation Properties

        public User User
        {
            get { return _user ?? (_user = new User()); }
            set { _user = value; }
        }

        public Role Role
        {
            get { return _role ?? (_role = new Role()); }
            set { _role = value; }
        }

        #endregion
    }
}