using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartBooking.Domain.Entities;

namespace SmartBooking.Infrastructure.Persistence.Configurations;

public class BusinessConfiguration : IEntityTypeConfiguration<Business>
{
  public void Configure(EntityTypeBuilder<Business> builder)
  {
    builder.HasKey(x => x.Id);

    builder.Property(x => x.Name)
        .IsRequired()
        .HasMaxLength(200);

    builder.Property(x => x.Description)
        .HasMaxLength(1000);

    builder.Property(x => x.Address)
        .IsRequired()
        .HasMaxLength(500);

    builder.Property(x => x.Phone)
        .IsRequired()
        .HasMaxLength(20);

    builder.Property(x => x.AvatarUrl)
        .HasMaxLength(500);

    builder.Property(x => x.BusinessType)
        .IsRequired();

    builder.Property(x => x.IsActive)
        .HasDefaultValue(true);

    // Quan hệ Business → Owner (User)
    // DeleteBehavior.Restrict: không cho xóa User nếu còn Business
    builder.HasOne(x => x.Owner)
        .WithMany()
        .HasForeignKey(x => x.OwnerId)
        .OnDelete(DeleteBehavior.Restrict);

    // Quan hệ Business → BusinessRoles (1:N)
    builder.HasMany(x => x.BusinessRoles)
        .WithOne(x => x.Business)
        .HasForeignKey(x => x.BusinessId)
        .OnDelete(DeleteBehavior.Cascade); // xóa Business → xóa hết Role

    // Quan hệ Business → BusinessUsers (1:N)
    builder.HasMany(x => x.BusinessUsers)
        .WithOne(x => x.Business)
        .HasForeignKey(x => x.BusinessId)
        .OnDelete(DeleteBehavior.Cascade); // xóa Business → xóa hết mapping

    builder.ToTable("Businesses");
  }
}