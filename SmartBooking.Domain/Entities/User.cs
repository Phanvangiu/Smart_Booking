using SmartBooking.Domain.Common;

namespace SmartBooking.Domain.Entities
{
  public class User : BaseEntity
  {
    public string Email { get; set; } = string.Empty;
    public string? Password { get; set; }


    public string FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? AvatarUrl { get; set; }


    public bool IsActive { get; set; } = true;
    public bool IsEmailVerified { get; set; } = false;

    // --- OAuth (Google login) ---
    public string? GoogleId { get; set; }

    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }


    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;
  }

}