using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartBooking.Domain.Entities;

namespace SmartBooking.Infrastructure.Persistence.Configurations;

public class BusinessUserConfiguration : IEntityTypeConfiguration<BusinessUser>
{
  public void Configure(EntityTypeBuilder<BusinessUser> builder)
  {
    builder.HasKey(x => x.Id);

    builder.Property(x => x.IsActive)
        .HasDefaultValue(true);

    builder.Property(x => x.JoinedAt)
        .IsRequired();

    // 1 User chỉ thuộc 1 Business — unique constraint
    builder.HasIndex(x => x.UserId)
        .IsUnique();

    // Quan hệ BusinessUser → User
    // Restrict: không xóa User nếu còn trong Business
    builder.HasOne(x => x.User)
        .WithMany()
        .HasForeignKey(x => x.UserId)
        .OnDelete(DeleteBehavior.Restrict);

    builder.ToTable("BusinessUsers");
  }
}