using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartBooking.Domain.Entities;

namespace SmartBooking.Infrastructure.Persistence.Configurations
{
  public class BusinessRoleConfiguration : IEntityTypeConfiguration<BusinessRole>
  {
    public void Configure(EntityTypeBuilder<BusinessRole> builder)
    {
      builder.HasKey(x => x.Id);
      builder.Property(x => x.Name)
          .IsRequired()
          .HasMaxLength(100);

      builder.Property(x => x.Description)
          .HasMaxLength(500);

      builder.Property(x => x.IsDefault)
          .HasDefaultValue(false);

      builder.HasMany(x => x.BusinessRolePermissions)
          .WithOne(x => x.BusinessRole)
          .HasForeignKey(x => x.BusinessRoleId)
          .OnDelete(DeleteBehavior.Cascade); // xóa Role → xóa hết permission mapping

      // Quan hệ BusinessRole → BusinessUsers (1:N)
      // Restrict: không xóa Role nếu còn Staff đang giữ
      builder.HasMany(x => x.BusinessUsers)
          .WithOne(x => x.BusinessRole)
          .HasForeignKey(x => x.BusinessRoleId)
          .OnDelete(DeleteBehavior.Restrict);

      builder.ToTable("BusinessRoles");
    }
  }
}