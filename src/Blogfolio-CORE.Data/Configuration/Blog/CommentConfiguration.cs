using Blogfolio_CORE.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Blogfolio.Data.Configuration
{
    internal class CommentConfiguration : IEntityTypeConfiguration<Comment>
    {
        public void Configure(EntityTypeBuilder<Comment> builder)
        {
            builder.ToTable("Comment");

            builder.HasKey(x => x.CommentId)
                .HasName("CommentId");

            builder.Property(x => x.Name)
                .HasColumnName("Name")
                .HasColumnType("varchar")
                .HasMaxLength(32)
                .IsRequired();

            builder.Property(x => x.Email)
                .HasColumnName("Email")
                .HasColumnType("varchar")
                .HasMaxLength(320)
                .IsRequired();

            builder.Property(x => x.Website)
                .HasColumnName("Website")
                .HasColumnType("varchar")
                .HasMaxLength(256)
                .IsRequired(false);

            builder.Property(x => x.Content)
                .HasColumnName("Content")
                .HasColumnType("varchar")
                .HasMaxLength(1000)
                .IsRequired();

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
        }
    }
}