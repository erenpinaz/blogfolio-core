using Blogfolio_CORE.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Blogfolio.Data.Configuration
{
    internal class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.ToTable("Category");

            builder.HasKey(x => x.CategoryId)
                .HasName("CategoryId");

            builder.Property(x => x.Name)
                .HasColumnName("Name")
                .HasColumnType("varchar")
                .HasMaxLength(32)
                .IsRequired();

            builder.Property(x => x.Slug)
                .HasColumnName("Slug")
                .HasColumnType("varchar")
                .HasMaxLength(32)
                .IsRequired();

            builder.HasIndex(x => x.Slug)
                .HasName("IX_Category_Slug")
                .IsUnique();

            builder.Property(x => x.DateCreated)
                .HasColumnName("DateCreated")
                .HasColumnType("timestamp")
                .IsRequired();

            builder.Property(x => x.DateModified)
                .HasColumnName("DateModified")
                .HasColumnType("timestamp")
                .IsRequired(false);
        }
    }
}