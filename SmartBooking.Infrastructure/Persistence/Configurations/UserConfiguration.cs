using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartBooking.Domain.Entities;

namespace SmartBooking.Infrastructure.Persistence.Configurations;

/// <summary>
/// Cấu hình chi tiết cách EF Core map User entity thành table "Users".
///
/// Tại sao dùng Fluent API thay vì Data Annotations?
/// → Giữ Domain entity sạch, không bị phụ thuộc vào EF Core attributes.
/// → User.cs chỉ là C# thuần — không có [Required], [MaxLength]...
/// → Mọi cấu hình DB tập trung ở đây, dễ tìm và sửa.
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
  public void Configure(EntityTypeBuilder<User> builder)
  {
    builder.ToTable("Users");

    // Primary Key — kế thừa từ BaseEntity
    builder.HasKey(u => u.Id);

    // --- Cấu hình từng column ---
    builder.Property(u => u.FullName)
        .IsRequired()
        .HasMaxLength(100);

    builder.Property(u => u.Email)
        .IsRequired()
        .HasMaxLength(256);

    // Email phải unique — không thể 2 user cùng email
    builder.HasIndex(u => u.Email)
        .IsUnique();

    builder.Property(u => u.Password)
        .HasMaxLength(512);
    // Nullable — vì Google login không có password

    builder.Property(u => u.PhoneNumber)
        .HasMaxLength(11);

    builder.Property(u => u.AvatarUrl)
        .HasMaxLength(500);

    builder.Property(u => u.GoogleId)
        .HasMaxLength(100);

    builder.Property(u => u.RefreshToken)
        .HasMaxLength(500);

    // --- Relationship: User n-1 Role ---
    builder.HasOne(u => u.Role)
        .WithMany(r => r.Users)
        .HasForeignKey(u => u.RoleId)
        .OnDelete(DeleteBehavior.Restrict);
    // Restrict: không cho xoá Role nếu còn User đang dùng
    // (tránh cascade delete xoá hết user)

    // --- Timestamps từ BaseEntity ---
    builder.Property(u => u.CreatedAt)
        .IsRequired();

    builder.Property(u => u.UpdatedAt)
        .IsRequired(false);
  }
}