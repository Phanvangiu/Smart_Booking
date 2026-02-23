using MediatR;
using SmartBooking.Application.Common;
using SmartBooking.Application.Features.Auth.DTOs;

namespace SmartBooking.Application.Features.Auth.Commands.Login;

/// <summary>
/// Đầu vào của use case Login.
/// Chỉ chứa data — không có logic.
/// </summary>
public class LoginCommand : IRequest<ApiResponse<AuthResponse>>
{
  public string Email { get; set; } = string.Empty;
  public string Password { get; set; } = string.Empty;
}