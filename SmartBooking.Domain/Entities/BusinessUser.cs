using SmartBooking.Domain.Common;

namespace SmartBooking.Domain.Entities;

/// <summary>
/// Mapping User thuộc Business nào, giữ Role gì.
/// 1 User chỉ thuộc 1 Business duy nhất.
/// </summary>
public class BusinessUser : BaseEntity
{
  public bool IsActive { get; set; } = true;
  public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

  // FKs
  public Guid UserId { get; set; }
  public Guid BusinessId { get; set; }
  public Guid BusinessRoleId { get; set; }

  // Navigation
  public User User { get; set; } = null!;
  public Business Business { get; set; } = null!;
  public BusinessRole BusinessRole { get; set; } = null!;
}