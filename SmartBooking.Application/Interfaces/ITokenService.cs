// SmartBooking.Application/Interfaces/ITokenService.cs
using System.Security.Claims;
using SmartBooking.Domain.Entities;

namespace SmartBooking.Application.Interfaces;

public interface ITokenService
{
  string GenerateAccessToken(User user);
  string GenerateRefreshToken();
  // Đọc token hết hạn để lấy claims — dùng cho RefreshToken flow
  ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}