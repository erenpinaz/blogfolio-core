using Blogfolio_CORE.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Blogfolio.Data.Configuration
{
    internal class MediaConfiguration : IEntityTypeConfiguration<Media>
    {
        public void Configure(EntityTypeBuilder<Media> builder)
        {
            builder.ToTable("Media");

            builder.HasKey(x => x.MediaId)
                .HasName("MediaId");

            builder.Property(x => x.Name)
                .HasColumnName("Name")
                .HasColumnType("varchar")
                .HasMaxLength(256)
                .IsRequired();

            builder.Property(x => x.Path)
                .HasColumnName("Path")
                .HasColumnType("varchar")
                .IsRequired();

            builder.Property(x => x.ThumbPath)
                .HasColumnName("ThumbPath")
                .HasColumnType("varchar")
                .IsRequired();

            builder.Property(x => x.Type)
                .HasColumnName("Type")
                .HasColumnType("varchar")
                .HasMaxLength(64);

            builder.Property(x => x.Size)
                .HasColumnName("Size")
                .HasColumnType("varchar")
                .HasMaxLength(32);

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