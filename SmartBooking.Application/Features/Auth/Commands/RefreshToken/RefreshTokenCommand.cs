using MediatR;
using SmartBooking.Application.Common;
using SmartBooking.Application.Features.Auth.DTOs;

namespace SmartBooking.Application.Features.Auth.Commands.RefreshToken;

/// <summary>
/// Client gửi lên cặp: Access Token cũ (có thể đã hết hạn) + Refresh Token còn hạn.
/// Handler sẽ verify và cấp cặp token mới.
/// </summary>
public class RefreshTokenCommand : IRequest<ApiResponse<AuthResponse>>
{
  public string AccessToken { get; set; } = string.Empty;
  public string RefreshToken { get; set; } = string.Empty;
}