using SmartBooking.Domain.Common;

namespace SmartBooking.Domain.Entities;

/// <summary>
/// Quyền hệ thống — seed sẵn, Owner không tạo được.
/// Owner chỉ ASSIGN Permission cho BusinessRole.
/// 
/// Naming convention: {resource}.{action}
/// booking.create, booking.view.all, service.edit, staff.manage...
/// </summary>
public class Permission : BaseEntity
{
  // "booking.create" — unique identifier dùng trong code
  public string Code { get; set; } = string.Empty;

  // "Tạo booking" — hiển thị trên UI
  public string Name { get; set; } = string.Empty;

  public string? Description { get; set; }

  // Nhóm để hiển thị UI: "Booking", "Service", "Staff", "Report"
  public string Group { get; set; } = string.Empty;

  // Navigation
  public ICollection<BusinessRolePermission> BusinessRolePermissions { get; set; } = new List<BusinessRolePermission>();
}