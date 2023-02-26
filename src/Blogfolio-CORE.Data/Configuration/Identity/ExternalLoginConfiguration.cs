using Blogfolio_CORE.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Blogfolio.Data.Configuration
{
    internal class ExternalLoginConfiguration : IEntityTypeConfiguration<ExternalLogin>
    {
        public void Configure(EntityTypeBuilder<ExternalLogin> builder)
        {
            builder.ToTable("ExternalLogin");

            builder.HasKey(x => new { x.LoginProvider, x.ProviderKey, x.UserId });

            builder.Property(x => x.LoginProvider)
                .HasColumnName("LoginProvider")
                .HasColumnType("varchar")
                .HasMaxLength(128)
                .IsRequired();

            builder.Property(x => x.ProviderKey)
                .HasColumnName("ProviderKey")
                .HasColumnType("varchar")
                .HasMaxLength(128)
                .IsRequired();

            builder.Property(x => x.UserId)
                .HasColumnName("UserId")
                .HasColumnType("uuid")
                .IsRequired();
        }
    }
}