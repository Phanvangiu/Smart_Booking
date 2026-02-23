using MediatR;
using SmartBooking.Application.Common;
using SmartBooking.Application.Features.Auth.DTOs;
using SmartBooking.Application.Interfaces;

namespace SmartBooking.Application.Features.Auth.Commands.RefreshToken;

/// <summary>
/// Luồng Refresh Token:
///
/// 1. Client gửi: Access Token cũ (hết hạn) + Refresh Token
/// 2. Handler đọc claims từ Access Token cũ → lấy UserId
/// 3. Tìm User trong DB, kiểm tra Refresh Token có khớp và còn hạn không
/// 4. Nếu hợp lệ → cấp cặp token mới, lưu Refresh Token mới (rotation)
/// 5. Refresh Token cũ bị invalidate ngay lập tức
///
/// Tại sao cần gửi kèm Access Token dù nó hết hạn?
/// → Để đọc UserId từ claims — không cần thêm parameter
/// → Validate đúng thuật toán ký — tránh token giả mạo
/// </summary>
public class RefreshTokenCommandHandler
    : IRequestHandler<RefreshTokenCommand, ApiResponse<AuthResponse>>
{
  private readonly IUnitOfWork _uow;
  private readonly ITokenService _tokenService;

  public RefreshTokenCommandHandler(IUnitOfWork uow, ITokenService tokenService)
  {
    _uow = uow;
    _tokenService = tokenService;
  }

  public async Task<ApiResponse<AuthResponse>> Handle(
      RefreshTokenCommand command,
      CancellationToken cancellationToken)
  {
    // Bước 1: Đọc claims từ Access Token đã hết hạn
    // GetPrincipalFromExpiredToken bỏ qua validate lifetime
    // nhưng vẫn validate chữ ký — token giả mạo sẽ bị reject
    var principal = _tokenService.GetPrincipalFromExpiredToken(command.AccessToken);
    if (principal is null)
      return ApiResponse<AuthResponse>.Fail("Access token không hợp lệ");

    // Bước 2: Lấy UserId từ claims
    var userIdClaim = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    if (!Guid.TryParse(userIdClaim, out var userId))
      return ApiResponse<AuthResponse>.Fail("Access token không hợp lệ");

    // Bước 3: Tìm User trong DB
    var user = await _uow.Users.GetByIdAsync(userId, cancellationToken);
    if (user is null || !user.IsActive)
      return ApiResponse<AuthResponse>.Fail("Tài khoản không tồn tại hoặc đã bị khoá");

    // Bước 4: Kiểm tra Refresh Token
    // Phải khớp với DB VÀ chưa hết hạn
    if (user.RefreshToken != command.RefreshToken)
      return ApiResponse<AuthResponse>.Fail("Refresh token không hợp lệ");

    if (user.RefreshTokenExpiryTime <= DateTime.UtcNow)
      return ApiResponse<AuthResponse>.Fail("Refresh token đã hết hạn, vui lòng đăng nhập lại");

    // Bước 5: Load Role
    var role = await _uow.Roles.GetByIdAsync(user.RoleId, cancellationToken);

    // Bước 6: Rotation — tạo cặp token mới, invalidate token cũ
    user.RefreshToken = _tokenService.GenerateRefreshToken();
    user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
    user.UpdatedAt = DateTime.UtcNow;

    _uow.Users.Update(user);
    await _uow.SaveChangesAsync(cancellationToken);

    // Bước 7: Tạo Access Token mới
    user.Role = role!;
    var newAccessToken = _tokenService.GenerateAccessToken(user);

    return ApiResponse<AuthResponse>.Success(
        new AuthResponse
        {
          AccessToken = newAccessToken,
          RefreshToken = user.RefreshToken,
          AccessTokenExpiry = DateTime.UtcNow.AddMinutes(60),
          UserId = user.Id,
          FullName = user.FullName,
          Email = user.Email,
          Role = role!.Name
        },
        "Làm mới token thành công");
  }
}