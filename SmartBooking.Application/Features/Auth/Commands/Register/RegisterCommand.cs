using MediatR;
using SmartBooking.Application.Common;
using SmartBooking.Application.Features.Auth.DTOs;

namespace SmartBooking.Application.Features.Auth.Commands.Register;

public record RegisterCommand : IRequest<ApiResponse<AuthResponse>>
// IRequest<T> — MediatR biết Handler nào cần xử lý và trả về kiểu gì
{
  public string FullName { get; set; } = string.Empty;
  public string Email { get; set; } = string.Empty;
  public string Password { get; set; } = string.Empty;
  public string? PhoneNumber { get; set; }
}