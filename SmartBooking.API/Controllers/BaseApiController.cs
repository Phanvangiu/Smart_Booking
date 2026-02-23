using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace SmartBooking.API.Controllers;

/// <summary>
/// Base class cho tất cả Controller trong hệ thống.
///
/// Tại sao cần BaseApiController?
/// → Tập trung các attribute dùng chung: [ApiController], [Route]
/// → Inject IMediator 1 lần — tất cả Controller con dùng luôn
/// → Sau này có thể thêm helper method dùng chung (lấy UserId từ claims...)
/// </summary>
[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
  private IMediator? _mediatorInstance;

  // Lazy inject qua HttpContext — không cần constructor inject ở mỗi Controller con
  protected IMediator _mediator =>
      _mediatorInstance ??= HttpContext.RequestServices.GetRequiredService<IMediator>();

  /// <summary>
  /// Lấy UserId của người dùng đang đăng nhập từ JWT claims.
  /// Dùng trong các endpoint cần [Authorize].
  /// </summary>
  protected Guid CurrentUserId
  {
    get
    {
      var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
      return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }
  }

  /// <summary>
  /// Lấy Role của người dùng đang đăng nhập từ JWT claims.
  /// </summary>
  protected string CurrentUserRole =>
      User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? string.Empty;
}