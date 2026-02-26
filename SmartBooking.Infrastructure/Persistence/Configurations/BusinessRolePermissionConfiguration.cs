using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartBooking.Domain.Entities;

namespace SmartBooking.Infrastructure.Persistence.Configurations;

public class BusinessRolePermissionConfiguration : IEntityTypeConfiguration<BusinessRolePermission>
{
  public void Configure(EntityTypeBuilder<BusinessRolePermission> builder)
  {
    // PK composite — không cần Id riêng
    builder.HasKey(x => new { x.BusinessRoleId, x.PermissionId });

    builder.Property(x => x.AssignedAt)
        .IsRequired();

    // Quan hệ → BusinessRole
    builder.HasOne(x => x.BusinessRole)
        .WithMany(x => x.BusinessRolePermissions)
        .HasForeignKey(x => x.BusinessRoleId)
        .OnDelete(DeleteBehavior.Cascade);

    // Quan hệ → Permission
    // Restrict: không xóa Permission nếu còn Role đang dùng
    builder.HasOne(x => x.Permission)
        .WithMany(x => x.BusinessRolePermissions)
        .HasForeignKey(x => x.PermissionId)
        .OnDelete(DeleteBehavior.Restrict);

    builder.ToTable("BusinessRolePermissions");
  }
}