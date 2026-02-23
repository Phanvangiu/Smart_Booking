using MediatR;
using SmartBooking.Application.Common;
using SmartBooking.Application.Features.Auth.DTOs;
using SmartBooking.Application.Interfaces;
using SmartBooking.Domain.Entities;

namespace SmartBooking.Application.Features.Auth.Commands.Register;

/// <summary>
/// Xử lý logic đăng ký tài khoản mới.
///
/// Handler KHÔNG biết:
///   - Database dùng SQL Server hay gì
///   - Password hash dùng BCrypt hay Argon2
///   - Token tạo như thế nào
/// Handler CHỈ biết:
///   - Gọi đúng interface, đúng thứ tự
///   - Trả về ApiResponse chuẩn
/// </summary>
public class RegisterCommandHandler
    : IRequestHandler<RegisterCommand, ApiResponse<AuthResponse>>
{
  private readonly IUnitOfWork _uow;
  private readonly ITokenService _tokenService;
  private readonly IPasswordService _passwordService;

  public RegisterCommandHandler(
      IUnitOfWork uow,
      ITokenService tokenService,
      IPasswordService passwordService)
  {
    _uow = uow;
    _tokenService = tokenService;
    _passwordService = passwordService;
  }

  public async Task<ApiResponse<AuthResponse>> Handle(
      RegisterCommand command,
      CancellationToken cancellationToken)
  {
    // Bước 1: Kiểm tra email đã tồn tại chưa
    var emailExists = await _uow.Users.ExistsAsync(
        u => u.Email == command.Email.ToLower().Trim(),
        cancellationToken);

    if (emailExists)
      return ApiResponse<AuthResponse>.Fail("Email đã được sử dụng");

    // Bước 2: Lấy Role mặc định Customer từ DB
    // Role phải được seed sẵn khi khởi động app
    var customerRole = await _uow.Roles.FirstOrDefaultAsync(
        r => r.Name == "Customer",
        cancellationToken);

    if (customerRole is null)
      return ApiResponse<AuthResponse>.Fail("Lỗi cấu hình hệ thống: không tìm thấy Role");

    // Bước 3: Tạo User entity mới
    // PasswordService hash password — Handler không biết thuật toán gì
    var user = new User
    {
      FullName = command.FullName.Trim(),
      Email = command.Email.ToLower().Trim(),
      Password = _passwordService.HashPassword(command.Password),
      PhoneNumber = command.PhoneNumber,
      RoleId = customerRole.Id,
      IsActive = true,
      IsEmailVerified = false
    };

    // Bước 4: Tạo Refresh Token và gán vào User
    user.RefreshToken = _tokenService.GenerateRefreshToken();
    user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

    // Bước 5: Lưu vào DB — UnitOfWork đảm bảo atomic
    await _uow.Users.AddAsync(user, cancellationToken);
    await _uow.SaveChangesAsync(cancellationToken);

    // Bước 6: Tạo Access Token và trả kết quả
    // Gán Role vào user để TokenService đọc được claim
    user.Role = customerRole;
    var accessToken = _tokenService.GenerateAccessToken(user);

    return ApiResponse<AuthResponse>.Success(
        new AuthResponse
        {
          AccessToken = accessToken,
          RefreshToken = user.RefreshToken,
          AccessTokenExpiry = DateTime.UtcNow.AddMinutes(60),
          UserId = user.Id,
          FullName = user.FullName,
          Email = user.Email,
          Role = customerRole.Name
        },
        "Đăng ký thành công");
  }
}