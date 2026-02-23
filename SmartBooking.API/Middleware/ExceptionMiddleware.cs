using FluentValidation;
using SmartBooking.Application.Common;
using System.Net;
using System.Text.Json;

namespace SmartBooking.API.Middleware;

/// <summary>
/// Global Exception Handler — bắt mọi exception chưa được xử lý trong app.
///
/// Tại sao cần Middleware này?
/// → Không để stack trace lộ ra ngoài (security risk)
/// → Đảm bảo mọi error response đều có format ApiResponse<T> chuẩn
/// → Controller không cần try/catch — code sạch hơn
///
/// Thứ tự xử lý trong pipeline:
/// Request → ExceptionMiddleware → Auth → Controller → Handler
///                ↑
///         Bắt exception từ mọi tầng phía trong
/// </summary>
public class ExceptionMiddleware
{
  private readonly RequestDelegate _next;
  private readonly ILogger<ExceptionMiddleware> _logger;

  public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
  {
    _next = next;
    _logger = logger;
  }

  public async Task InvokeAsync(HttpContext context)
  {
    try
    {
      await _next(context); // Tiếp tục pipeline bình thường
    }
    catch (ValidationException ex)
    {
      // FluentValidation throw khi data không hợp lệ
      // ValidationBehavior trong Application pipeline throw exception này
      _logger.LogWarning("Validation failed: {Errors}",
          string.Join(", ", ex.Errors.Select(e => e.ErrorMessage)));

      await WriteErrorResponse(
          context,
          HttpStatusCode.BadRequest,
          ex.Errors.Select(e => e.ErrorMessage).ToList());
    }
    catch (UnauthorizedAccessException ex)
    {
      // Người dùng không có quyền truy cập resource
      _logger.LogWarning("Unauthorized access: {Message}", ex.Message);

      await WriteErrorResponse(
          context,
          HttpStatusCode.Unauthorized,
          new List<string> { "Bạn không có quyền thực hiện hành động này" });
    }
    catch (KeyNotFoundException ex)
    {
      // Không tìm thấy resource (User, Booking...)
      _logger.LogWarning("Resource not found: {Message}", ex.Message);

      await WriteErrorResponse(
          context,
          HttpStatusCode.NotFound,
          new List<string> { ex.Message });
    }
    catch (Exception ex)
    {
      // Lỗi không mong đợi — log đầy đủ để debug
      // Nhưng KHÔNG trả stack trace về client
      _logger.LogError(ex, "Unhandled exception occurred");

      await WriteErrorResponse(
          context,
          HttpStatusCode.InternalServerError,
          new List<string> { "Đã xảy ra lỗi hệ thống. Vui lòng thử lại sau." });
    }
  }

  private static async Task WriteErrorResponse(
      HttpContext context,
      HttpStatusCode statusCode,
      List<string> errors)
  {
    context.Response.ContentType = "application/json";
    context.Response.StatusCode = (int)statusCode;

    var response = ApiResponse<object>.Fail(errors);

    var options = new JsonSerializerOptions
    {
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase
      // camelCase để frontend đọc dễ hơn: isSuccess, errors
    };

    await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
  }
}