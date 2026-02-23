using MediatR;
using SmartBooking.Application.Common;
using SmartBooking.Application.Features.Auth.DTOs;
using SmartBooking.Application.Interfaces;

namespace SmartBooking.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler
    : IRequestHandler<LoginCommand, ApiResponse<AuthResponse>>
{
  private readonly IUnitOfWork _uow;
  private readonly ITokenService _tokenService;
  private readonly IPasswordService _passwordService;

  public LoginCommandHandler(
      IUnitOfWork uow,
      ITokenService tokenService,
      IPasswordService passwordService)
  {
    _uow = uow;
    _tokenService = tokenService;
    _passwordService = passwordService;
  }

  public async Task<ApiResponse<AuthResponse>> Handle(
      LoginCommand command,
      CancellationToken cancellationToken)
  {
    // Bước 1: Tìm user theo email
    var user = await _uow.Users.FirstOrDefaultAsync(
        u => u.Email == command.Email.ToLower().Trim(),
        cancellationToken);

    // Trả về cùng 1 message cho cả 2 trường hợp:
    // - Email không tồn tại
    // - Password sai
    // Lý do: không để attacker biết email có tồn tại hay không
    if (user is null)
      return ApiResponse<AuthResponse>.Fail("Email hoặc mật khẩu không đúng");

    // Bước 2: Kiểm tra tài khoản có active không
    if (!user.IsActive)
      return ApiResponse<AuthResponse>.Fail("Tài khoản đã bị khoá");

    // Bước 3: Kiểm tra password
    // User đăng nhập Google sẽ không có PasswordHash
    if (user.Password is null)
      return ApiResponse<AuthResponse>.Fail("Tài khoản này đăng nhập bằng Google");

    var isPasswordValid = _passwordService.VerifyPassword(command.Password, user.Password);
    if (!isPasswordValid)
      return ApiResponse<AuthResponse>.Fail("Email hoặc mật khẩu không đúng");

    // Bước 4: Load Role để tạo token có claim đúng
    var role = await _uow.Roles.GetByIdAsync(user.RoleId, cancellationToken);

    // Bước 5: Rotation Refresh Token
    // Mỗi lần login → tạo refresh token mới → invalidate token cũ
    user.RefreshToken = _tokenService.GenerateRefreshToken();
    user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
    user.UpdatedAt = DateTime.UtcNow;

    _uow.Users.Update(user);
    await _uow.SaveChangesAsync(cancellationToken);

    // Bước 6: Tạo Access Token
    user.Role = role!;
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
          Role = role!.Name
        },
        "Đăng nhập thành công");
  }
}