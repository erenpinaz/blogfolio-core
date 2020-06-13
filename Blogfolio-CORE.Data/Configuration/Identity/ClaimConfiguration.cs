using Blogfolio_CORE.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Blogfolio.Data.Configuration
{
    internal class ClaimConfiguration : IEntityTypeConfiguration<Claim>
    {
        public void Configure(EntityTypeBuilder<Claim> builder)
        {
            builder.ToTable("Claim");

            builder.HasKey(x => x.ClaimId)
                .HasName("ClaimId");

            builder.Property(x => x.UserId)
                .HasColumnName("UserId")
                .HasColumnType("uuid")
                .IsRequired();

            builder.Property(x => x.ClaimType)
                .HasColumnName("ClaimType")
                .HasColumnType("varchar")
                .IsRequired(false);

            builder.Property(x => x.ClaimValue)
                .HasColumnName("ClaimValue")
                .HasColumnType("varchar")
                .IsRequired(false);
        }
    }
}