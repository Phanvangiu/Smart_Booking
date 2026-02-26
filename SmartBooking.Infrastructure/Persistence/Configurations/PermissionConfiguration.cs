using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartBooking.Domain.Constants;
using SmartBooking.Domain.Entities;

namespace SmartBooking.Infrastructure.Persistence.Configurations;

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
  public void Configure(EntityTypeBuilder<Permission> builder)
  {
    builder.HasKey(x => x.Id);

    builder.Property(x => x.Code)
        .IsRequired()
        .HasMaxLength(100);

    builder.Property(x => x.Name)
        .IsRequired()
        .HasMaxLength(200);

    builder.Property(x => x.Description)
        .HasMaxLength(500);

    builder.Property(x => x.Group)
        .IsRequired()
        .HasMaxLength(50);

    // Code phải unique — dùng trong code để check permission
    builder.HasIndex(x => x.Code)
        .IsUnique();

    // Seed tất cả permissions — dùng Guid cố định
    builder.HasData(SeedPermissions());

    builder.ToTable("Permissions");
  }

  private static IEnumerable<Permission> SeedPermissions()
  {
    var now = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    return new List<Permission>
        {
            // ── Booking ──────────────────────────────────────────
            new() { Id = Guid.Parse("a1000001-0000-0000-0000-000000000001"), Code = PermissionCodes.Booking.ViewAll,      Name = "Xem tất cả booking",        Group = "Booking", CreatedAt = now, UpdatedAt = now },
            new() { Id = Guid.Parse("a1000001-0000-0000-0000-000000000002"), Code = PermissionCodes.Booking.ViewOwn,      Name = "Xem booking của mình",      Group = "Booking", CreatedAt = now, UpdatedAt = now },
            new() { Id = Guid.Parse("a1000001-0000-0000-0000-000000000003"), Code = PermissionCodes.Booking.Create,       Name = "Tạo booking",               Group = "Booking", CreatedAt = now, UpdatedAt = now },
            new() { Id = Guid.Parse("a1000001-0000-0000-0000-000000000004"), Code = PermissionCodes.Booking.Cancel,       Name = "Hủy booking",               Group = "Booking", CreatedAt = now, UpdatedAt = now },
            new() { Id = Guid.Parse("a1000001-0000-0000-0000-000000000005"), Code = PermissionCodes.Booking.UpdateStatus, Name = "Cập nhật trạng thái booking", Group = "Booking", CreatedAt = now, UpdatedAt = now },

            // ── Service ──────────────────────────────────────────
            new() { Id = Guid.Parse("a2000002-0000-0000-0000-000000000001"), Code = PermissionCodes.Service.View,   Name = "Xem danh sách dịch vụ", Group = "Service", CreatedAt = now, UpdatedAt = now },
            new() { Id = Guid.Parse("a2000002-0000-0000-0000-000000000002"), Code = PermissionCodes.Service.Create, Name = "Tạo dịch vụ mới",       Group = "Service", CreatedAt = now, UpdatedAt = now },
            new() { Id = Guid.Parse("a2000002-0000-0000-0000-000000000003"), Code = PermissionCodes.Service.Edit,   Name = "Sửa dịch vụ",           Group = "Service", CreatedAt = now, UpdatedAt = now },
            new() { Id = Guid.Parse("a2000002-0000-0000-0000-000000000004"), Code = PermissionCodes.Service.Delete, Name = "Xóa dịch vụ",           Group = "Service", CreatedAt = now, UpdatedAt = now },

            // ── Staff ─────────────────────────────────────────────
            new() { Id = Guid.Parse("a3000003-0000-0000-0000-000000000001"), Code = PermissionCodes.Staff.Invite,     Name = "Mời nhân viên",          Group = "Staff", CreatedAt = now, UpdatedAt = now },
            new() { Id = Guid.Parse("a3000003-0000-0000-0000-000000000002"), Code = PermissionCodes.Staff.Remove,     Name = "Xóa nhân viên",          Group = "Staff", CreatedAt = now, UpdatedAt = now },
            new() { Id = Guid.Parse("a3000003-0000-0000-0000-000000000003"), Code = PermissionCodes.Staff.RoleAssign, Name = "Gán role cho nhân viên", Group = "Staff", CreatedAt = now, UpdatedAt = now },

            // ── Role ──────────────────────────────────────────────
            new() { Id = Guid.Parse("a4000004-0000-0000-0000-000000000001"), Code = PermissionCodes.Role.Create,           Name = "Tạo role mới",               Group = "Role", CreatedAt = now, UpdatedAt = now },
            new() { Id = Guid.Parse("a4000004-0000-0000-0000-000000000002"), Code = PermissionCodes.Role.Edit,             Name = "Sửa role",                   Group = "Role", CreatedAt = now, UpdatedAt = now },
            new() { Id = Guid.Parse("a4000004-0000-0000-0000-000000000003"), Code = PermissionCodes.Role.Delete,           Name = "Xóa role",                   Group = "Role", CreatedAt = now, UpdatedAt = now },
            new() { Id = Guid.Parse("a4000004-0000-0000-0000-000000000004"), Code = PermissionCodes.Role.PermissionAssign, Name = "Gán permission cho role",    Group = "Role", CreatedAt = now, UpdatedAt = now },

            // ── Report ────────────────────────────────────────────
            new() { Id = Guid.Parse("a5000005-0000-0000-0000-000000000001"), Code = PermissionCodes.Report.View,   Name = "Xem báo cáo",     Group = "Report", CreatedAt = now, UpdatedAt = now },
            new() { Id = Guid.Parse("a5000005-0000-0000-0000-000000000002"), Code = PermissionCodes.Report.Export, Name = "Xuất báo cáo",    Group = "Report", CreatedAt = now, UpdatedAt = now },

            // ── Business Settings ────────────────────────────────
            new() { Id = Guid.Parse("a6000006-0000-0000-0000-000000000001"), Code = PermissionCodes.BusinessSettings.Manage, Name = "Cấu hình thông tin business", Group = "Business", CreatedAt = now, UpdatedAt = now },
            new() { Id = Guid.Parse("a6000006-0000-0000-0000-000000000002"), Code = PermissionCodes.BusinessSettings.Hours,  Name = "Cấu hình giờ làm việc",      Group = "Business", CreatedAt = now, UpdatedAt = now },
        };
  }
}