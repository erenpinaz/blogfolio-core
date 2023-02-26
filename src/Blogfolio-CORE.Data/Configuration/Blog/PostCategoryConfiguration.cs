using Blogfolio_CORE.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Blogfolio.Data.Configuration
{
    internal class PostCategoryConfiguration : IEntityTypeConfiguration<PostCategory>
    {
        public void Configure(EntityTypeBuilder<PostCategory> builder)
        {
            builder.ToTable("PostCategory");

            builder.HasKey(x => new { x.PostId, x.CategoryId });

            builder.HasOne(x => x.Post)
                .WithMany(x => x.PostCategories)
                .HasForeignKey(x => x.PostId);

            builder.HasOne(x => x.Category)
                .WithMany(x => x.PostCategories)
                .HasForeignKey(x => x.CategoryId);
        }
    }
}