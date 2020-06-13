using Microsoft.EntityFrameworkCore;
using Blogfolio_CORE.Models;
using Blogfolio.Data.Configuration;

namespace Blogfolio_CORE.Data
{
    public class BlogfolioContext : DbContext
    {
        public BlogfolioContext(DbContextOptions<BlogfolioContext> options)
            : base(options)
        {
        }

        #region Entity Configuration

        // Identity
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<ExternalLogin> ExternalLogins { get; set; }

        // Blog
        public DbSet<Post> Posts { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<PostCategory> PostCategories { get; set; }
        public DbSet<Comment> Comments { get; set; }

        // Portfolio
        public DbSet<Project> Projects { get; set; }

        // Library
        public DbSet<Media> Medias { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Identity
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new RoleConfiguration());
            modelBuilder.ApplyConfiguration(new UserRoleConfiguration());
            modelBuilder.ApplyConfiguration(new ExternalLoginConfiguration());
            modelBuilder.ApplyConfiguration(new ClaimConfiguration());

            // Blog
            modelBuilder.ApplyConfiguration(new PostConfiguration());
            modelBuilder.ApplyConfiguration(new CategoryConfiguration());
            modelBuilder.ApplyConfiguration(new PostCategoryConfiguration());
            modelBuilder.ApplyConfiguration(new CommentConfiguration());

            // Portfolio
            modelBuilder.ApplyConfiguration(new ProjectConfiguration());

            // Library
            modelBuilder.ApplyConfiguration(new MediaConfiguration());

            base.OnModelCreating(modelBuilder);
        }

        #endregion
    }
}