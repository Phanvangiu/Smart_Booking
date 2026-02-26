using SmartBooking.Domain.Common;

namespace SmartBooking.Domain.Entities;

/// <summary>
/// Role do Owner tự tạo trong Business của mình.
/// Ví dụ Spa: Manager, Therapist, Lễ tân
/// Ví dụ Phòng khám: Bác sĩ, Y tá, Lễ tân
/// Hoàn toàn tách biệt với system Role (Admin, Customer...)
/// </summary>
public class BusinessRole : BaseEntity
{
  public string Name { get; set; } = string.Empty;
  public string? Description { get; set; }

  // true = role mặc định gán cho staff mới được invite
  public bool IsDefault { get; set; } = false;

  // FK
  public Guid BusinessId { get; set; }

  // Navigation
  public Business Business { get; set; } = null!;
  public ICollection<BusinessUser> BusinessUsers { get; set; } = new List<BusinessUser>();
  public ICollection<BusinessRolePermission> BusinessRolePermissions { get; set; } = new List<BusinessRolePermission>();
}