using Blogfolio_CORE.Models;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Blogfolio_CORE.Areas.Admin.Identity
{
    public class IdentityResult
    {
        public User User { get; set; }
        public bool Success { get; set; }
        public string Error { get; set; }

        public IdentityResult(User user = null, bool success = false, string error = null)
        {
            this.User = user;
            this.Success = success;
            this.Error = error;
        }
    }

    public interface IUserStore
    {
        Task<IdentityResult> CreateAsync(User user, string secret, string roleName);
        Task<IdentityResult> LoginAsync(HttpContext httpContext, string username, string secret, bool persistent);
        Task LogoutAsync(HttpContext httpContext);
        Task<bool> UserExistsAsync(string username);
    }
}