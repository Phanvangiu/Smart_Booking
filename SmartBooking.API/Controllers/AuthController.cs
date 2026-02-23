using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartBooking.Application.Features.Auth.Commands.Login;
using SmartBooking.Application.Features.Auth.Commands.RefreshToken;
using SmartBooking.Application.Features.Auth.Commands.Register;

namespace SmartBooking.API.Controllers;

/// <summary>
/// Xử lý tất cả endpoint liên quan Authentication.
/// Kế thừa BaseApiController → có sẵn _mediator, không cần inject thủ công.
///
/// Tất cả endpoint ở đây là [AllowAnonymous] vì:
/// → Chưa có token thì mới cần register/login
/// → RefreshToken dùng refresh token riêng, không cần JWT
/// </summary>
public class AuthController : BaseApiController
{
  // Route tự động: api/auth/register, api/auth/login, api/auth/refresh-token
  // Vì BaseApiController đã có [Route("api/[controller]")]
  // [controller] = "Auth" (bỏ chữ "Controller")

  /// <summary>
  /// Đăng ký tài khoản mới.
  /// Role mặc định: Customer.
  /// </summary>
  [HttpPost("register")]
  [AllowAnonymous]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  public async Task<IActionResult> Register(
      [FromBody] RegisterCommand command,
      CancellationToken cancellationToken)
  {
    var result = await _mediator.Send(command, cancellationToken);
    return result.IsSuccess ? Ok(result) : BadRequest(result);
  }

  /// <summary>
  /// Đăng nhập bằng email và password.
  /// Trả về Access Token (60 phút) + Refresh Token (7 ngày).
  /// </summary>
  [HttpPost("login")]
  [AllowAnonymous]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  public async Task<IActionResult> Login(
      [FromBody] LoginCommand command,
      CancellationToken cancellationToken)
  {
    var result = await _mediator.Send(command, cancellationToken);
    return result.IsSuccess ? Ok(result) : Unauthorized(result);
  }

  /// <summary>
  /// Cấp lại Access Token mới khi hết hạn.
  /// Gửi kèm Access Token cũ + Refresh Token còn hạn.
  /// </summary>
  [HttpPost("refresh-token")]
  [AllowAnonymous]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  public async Task<IActionResult> RefreshToken(
      [FromBody] RefreshTokenCommand command,
      CancellationToken cancellationToken)
  {
    var result = await _mediator.Send(command, cancellationToken);
    return result.IsSuccess ? Ok(result) : Unauthorized(result);
  }

  [HttpGet("get")]
  public async Task<IActionResult> Get()
  {
    return Ok("Vỹ Bê Đê");
  }
}