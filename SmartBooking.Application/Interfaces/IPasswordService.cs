namespace SmartBooking.Application.Interfaces;

/// <summary>
/// Contract để hash và verify password.
/// Application chỉ biết "cần hash" — không quan tâm dùng BCrypt hay gì.
/// Infrastructure sẽ implement bằng BCrypt.Net.
/// </summary>
public interface IPasswordService
{
  /// <summary>
  /// Hash plain text password trước khi lưu vào DB.
  /// </summary>
  string HashPassword(string password);

  /// <summary>
  /// Kiểm tra password người dùng nhập có khớp với hash trong DB không.
  /// </summary>
  bool VerifyPassword(string password, string hash);
}
