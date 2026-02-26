using SmartBooking.Domain.Common;
using SmartBooking.Domain.Enums;

namespace SmartBooking.Domain.Entities;

public class Business : BaseEntity
{
  public string Name { get; set; } = string.Empty;
  public string? Description { get; set; }
  public string Address { get; set; } = string.Empty;
  public string Phone { get; set; } = string.Empty;
  public string? AvatarUrl { get; set; }
  public BusinessType BusinessType { get; set; }
  public bool IsActive { get; set; } = true;

  // FK → Owner là User có Role BusinessOwner
  public Guid OwnerId { get; set; }

  // Navigation properties
  public User Owner { get; set; } = null!;
  public ICollection<BusinessRole> BusinessRoles { get; set; } = new List<BusinessRole>();
  public ICollection<BusinessUser> BusinessUsers { get; set; } = new List<BusinessUser>();
}