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
  }
}