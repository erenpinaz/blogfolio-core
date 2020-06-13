using Blogfolio_CORE.Data;
using Blogfolio_CORE.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Claim = System.Security.Claims.Claim;

namespace Blogfolio_CORE.Areas.Admin.Identity
{
    public class UserStore : IUserStore
    {
        private readonly BlogfolioContext _context;

        public UserStore(BlogfolioContext context)
        {
            _context = context;
        }

        public async Task<IdentityResult> CreateAsync(User user, string secret, string roleName)
        {
            try
            {
                CreatePasswordHash(secret, out byte[] passwordHash, out byte[] passwordSalt);

                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;

                _context.Users.Add(user);

                var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
                if (role == null)
                {
                    role = new Role { Name = roleName };
                    _context.Roles.Add(role);
                }

                var userRole = new UserRole { User = user, Role = role };
                _context.UserRoles.Add(userRole);

                await _context.SaveChangesAsync();

                return new IdentityResult(user, true);
            }
            catch
            {
                return new IdentityResult(user, false, "US1001");
            }
        }

        public async Task<IdentityResult> LoginAsync(HttpContext httpContext, string username, string secret, bool persistent)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(x => x.UserName == username);
                if (user == null)
                    return new IdentityResult(user, false, "US2001");

                if (!VerifyPassword(secret, user.PasswordHash, user.PasswordSalt))
                    return new IdentityResult(user, false, "US2002");

                var identity = new ClaimsIdentity(GetUserClaims(user), CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties() { IsPersistent = persistent });

                return new IdentityResult(user, true);
            }
            catch
            {
                return new IdentityResult(null, false, "US2001");
            }
        }

        public async Task LogoutAsync(HttpContext httpContext)
        {
            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }

        public async Task<bool> UserExistsAsync(string username)
        {
            if (await _context.Users.AnyAsync(x => x.UserName == username))
                return true;
            return false;
        }

        #region Helpers

        private IEnumerable<Claim> GetUserClaims(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Name)
            };

            var userRoles = _context.UserRoles.Where(x => x.UserId == user.UserId).Include(x => x.Role).Select(x => x.Role.Name);
            claims.AddRange(userRoles.Select(x => new Claim(ClaimTypes.Role, x)));

            return claims;
        }

        private bool VerifyPassword(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != passwordHash[i]) return false;
                }
            }
            return true;
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        #endregion
    }
}
