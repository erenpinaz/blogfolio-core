using Blogfolio_CORE.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Blogfolio.Data.Configuration
{
    internal class PostConfiguration : IEntityTypeConfiguration<Post>
    {
        public void Configure(EntityTypeBuilder<Post> builder)
        {
            builder.ToTable("Post");

            builder.HasKey(x => x.PostId)
                .HasName("PostId");

            builder.Property(x => x.PostId)
                .ValueGeneratedOnAdd();

            builder.Property(x => x.Title)
                .HasColumnName("Title")
                .HasColumnType("varchar")
                .HasMaxLength(64)
                .IsRequired();

            builder.Property(x => x.Summary)
                .HasColumnName("Summary")
                .HasColumnType("varchar")
                .HasMaxLength(320)
                .IsRequired();

            builder.Property(x => x.Content)
                .HasColumnName("Content")
                .HasColumnType("text")
                .IsRequired();

            builder.Property(x => x.Slug)
                .HasColumnName("Slug")
                .HasColumnType("varchar")
                .HasMaxLength(64)
                .IsRequired();

            builder.HasIndex(x => x.Slug)
                .HasName("IX_Post_Slug")
                .IsUnique();

            builder.Property(x => x.Status)
                .HasColumnName("Status")
                .IsRequired();

            builder.Property(x => x.DateCreated)
                .HasColumnName("DateCreated")
                .HasColumnType("timestamp")
                .IsRequired();

            builder.Property(x => x.DateModified)
                .HasColumnName("DateModified")
                .HasColumnType("timestamp")
                .IsRequired(false);

            builder.HasMany(x => x.Comments)
                .WithOne(x => x.Post)
                .HasForeignKey(x => x.PostId)
                .IsRequired();
        }
    }
}