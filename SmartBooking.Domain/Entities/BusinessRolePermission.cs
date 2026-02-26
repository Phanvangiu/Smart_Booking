namespace SmartBooking.Domain.Entities;

/// <summary>
/// Junction table N:M giữa BusinessRole và Permission.
/// BusinessRole có thể có nhiều Permission.
/// Permission có thể thuộc nhiều BusinessRole.
/// 
/// PK composite = (BusinessRoleId + PermissionId)
/// → Không dùng BaseEntity vì không cần Id riêng
/// </summary>
public class BusinessRolePermission
{
  public Guid BusinessRoleId { get; set; }
  public Guid PermissionId { get; set; }
  public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

  // Navigation
  public BusinessRole BusinessRole { get; set; } = null!;
  public Permission Permission { get; set; } = null!;
}