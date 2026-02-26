using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartBooking.Domain.Entities;

namespace SmartBooking.Infrastructure.Persistence.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
  public void Configure(EntityTypeBuilder<Role> builder)
  {
    builder.ToTable("Roles");

    builder.HasKey(r => r.Id);

    builder.Property(r => r.Name)
        .IsRequired()
        .HasMaxLength(50);

    // Role name phải unique
    builder.HasIndex(r => r.Name)
        .IsUnique();

    builder.Property(r => r.Description)
        .HasMaxLength(200);

    builder.Property(r => r.CreatedAt)
        .IsRequired();
    builder.HasData(SeedRoles());
  }

  private static IEnumerable<Role> SeedRoles()
  {
    var seedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc); // cố định
    return new List<Role>
      {
        new Role
        {
          Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
          Name = "Admin",
          Description = "Quản trị viên hệ thống",
          CreatedAt = seedDate
        },
        new Role
        {
          Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
          Name = "BusinessOwner",
          Description = "Chủ doanh nghiệp",
          CreatedAt = seedDate
        },
        new Role
        {
          Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
          Name = "Staff",
          Description = "Nhân viên",
          CreatedAt = seedDate
        },
        new Role
        {
          Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
          Name = "Customer",
          Description = "Khách hàng",
          CreatedAt = seedDate
        }
      };
  }
}