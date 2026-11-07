using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Waytokaaba.Core.Entities;

namespace Waytokaaba.Infrastructure.Persistence.Configurations;

internal sealed class InquiryConfiguration : IEntityTypeConfiguration<Inquiry>
{
    public void Configure(EntityTypeBuilder<Inquiry> b)
    {
        b.ToTable("inquiries", t =>
        {
            t.HasCheckConstraint("ck_inquiries_dates", "check_in IS NULL OR check_out IS NULL OR check_out > check_in");
            t.HasCheckConstraint("ck_inquiries_guests", "guests IS NULL OR guests BETWEEN 1 AND 50");
        });
        b.Property(i => i.CustomerName).HasMaxLength(120).IsRequired();
        b.Property(i => i.Email).HasMaxLength(200).IsRequired();
        b.Property(i => i.Phone).HasMaxLength(30).IsRequired();
        b.Property(i => i.Message).HasMaxLength(2000).IsRequired();

        b.HasIndex(i => new { i.Status, i.CreatedAt });
        b.HasIndex(i => i.CreatedAt);

        // Inquiries are kept for the business record even if the listing is later deleted.
        b.HasOne(i => i.Package).WithMany().HasForeignKey(i => i.PackageId).OnDelete(DeleteBehavior.SetNull);
        b.HasOne(i => i.Hotel).WithMany().HasForeignKey(i => i.HotelId).OnDelete(DeleteBehavior.SetNull);
    }
}

internal sealed class AdminUserConfiguration : IEntityTypeConfiguration<AdminUser>
{
    public void Configure(EntityTypeBuilder<AdminUser> b)
    {
        b.ToTable("admin_users");
        b.Property(u => u.Email).HasMaxLength(200).IsRequired();
        b.HasIndex(u => u.Email).IsUnique();
        b.Property(u => u.PasswordHash).HasMaxLength(500).IsRequired();
    }
}
