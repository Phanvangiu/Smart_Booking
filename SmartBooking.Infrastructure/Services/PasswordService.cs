using SmartBooking.Application.Interfaces;

namespace SmartBooking.Infrastructure.Services;

/// <summary>
/// Implement IPasswordService bằng BCrypt.Net.
///
/// Tại sao BCrypt?
/// → Tự động thêm "salt" ngẫu nhiên → 2 user cùng password → hash khác nhau
/// → Work factor (cost) có thể tăng khi CPU mạnh hơn
/// → Được thiết kế để chậm có chủ ý → brute force tốn kém hơn
///
/// Application không biết gì về BCrypt.
/// Nếu sau này muốn đổi sang Argon2, chỉ sửa file này.
/// </summary>
public class PasswordService : IPasswordService
{
  // Work factor 12 = 2^12 iterations
  // Cân bằng giữa security và performance
  // Tăng lên 14-16 cho hệ thống yêu cầu bảo mật cao hơn
  private const int WorkFactor = 12;

  public string HashPassword(string password)
      => BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);

  public bool VerifyPassword(string password, string hash)
      => BCrypt.Net.BCrypt.Verify(password, hash);
}