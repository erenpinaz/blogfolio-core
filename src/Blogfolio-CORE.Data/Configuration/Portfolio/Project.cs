using Blogfolio_CORE.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Blogfolio.Data.Configuration
{
    internal class ProjectConfiguration : IEntityTypeConfiguration<Project>
    {
        public void Configure(EntityTypeBuilder<Project> builder)
        {
            builder.ToTable("Project");

            builder.HasKey(x => x.ProjectId)
                .HasName("ProjectId");

            builder.Property(x => x.Name)
                .HasColumnName("Name")
                .HasColumnType("varchar")
                .HasMaxLength(64)
                .IsRequired();

            builder.Property(x => x.Image)
                .HasColumnName("Image")
                .HasColumnType("varchar")
                .IsRequired();

            builder.Property(x => x.Description)
                .HasColumnName("Description")
                .HasColumnType("text")
                .IsRequired();

            builder.Property(x => x.Slug)
                .HasColumnName("Slug")
                .HasColumnType("varchar")
                .HasMaxLength(64)
                .IsRequired();

            builder.HasIndex(x => x.Slug)
                .HasName("IX_Project_Slug")
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
        }
    }
}