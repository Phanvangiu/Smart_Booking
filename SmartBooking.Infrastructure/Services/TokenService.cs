using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SmartBooking.Application.Interfaces;
using SmartBooking.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SmartBooking.Infrastructure.Services;

/// <summary>
/// Implement ITokenService — tạo và đọc JWT token.
///
/// Access Token:
///   → Sống ngắn (60 phút)
///   → Chứa claims: userId, email, role
///   → Client gửi trong header: Authorization: Bearer {token}
///
/// Refresh Token:
///   → Sống dài (7 ngày)
///   → Random string, lưu trong DB
///   → Dùng để cấp Access Token mới khi hết hạn
///   → Rotation: mỗi lần dùng → tạo cái mới, invalidate cái cũ
/// </summary>
public class TokenService : ITokenService
{
  private readonly IConfiguration _config;

  public TokenService(IConfiguration config)
  {
    _config = config;
  }

  public string GenerateAccessToken(User user)
  {
    var jwtSettings = _config.GetSection("JwtSettings");
    var secretKey = jwtSettings["SecretKey"]!;
    var issuer = jwtSettings["Issuer"]!;
    var audience = jwtSettings["Audience"]!;
    var expirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"] ?? "60");

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
    var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    // Claims — thông tin được nhúng vào token
    // Client decode token sẽ đọc được những thông tin này
    var claims = new[]
    {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            // Jti: JWT ID — unique mỗi token, dùng để blacklist nếu cần
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Role, user.Role?.Name ?? string.Empty),
            // ClaimTypes.Role → [Authorize(Roles = "Admin")] sẽ đọc claim này
        };

    var token = new JwtSecurityToken(
        issuer: issuer,
        audience: audience,
        claims: claims,
        expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
        signingCredentials: credentials);

    return new JwtSecurityTokenHandler().WriteToken(token);
  }

  public string GenerateRefreshToken()
  {
    // 64 bytes random → base64 string
    // Không thể đoán được, không chứa thông tin nhạy cảm
    var randomBytes = new byte[64];
    using var rng = RandomNumberGenerator.Create();
    rng.GetBytes(randomBytes);
    return Convert.ToBase64String(randomBytes);
  }

  public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
  {
    var jwtSettings = _config.GetSection("JwtSettings");
    var secretKey = jwtSettings["SecretKey"]!;

    var tokenValidationParameters = new TokenValidationParameters
    {
      ValidateIssuer = true,
      ValidateAudience = true,
      ValidateIssuerSigningKey = true,
      ValidIssuer = jwtSettings["Issuer"],
      ValidAudience = jwtSettings["Audience"],
      IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(secretKey)),

      // Quan trọng: bỏ qua validate thời gian hết hạn
      // Vì method này dùng để đọc token ĐÃ hết hạn
      ValidateLifetime = false
    };

    try
    {
      var handler = new JwtSecurityTokenHandler();
      var principal = handler.ValidateToken(
          token,
          tokenValidationParameters,
          out var securityToken);

      // Kiểm tra đúng thuật toán ký
      if (securityToken is not JwtSecurityToken jwtToken ||
          !jwtToken.Header.Alg.Equals(
              SecurityAlgorithms.HmacSha256,
              StringComparison.InvariantCultureIgnoreCase))
        return null;

      return principal;
    }
    catch
    {
      return null; // Token không hợp lệ
    }
  }
}