using FluentValidation;
using MediatR;

namespace SmartBooking.Application.Common.Behaviours;

/// <summary>
/// MediatR Pipeline Behavior — tự động chạy trước mỗi Handler.
/// Luồng: Request → ValidationBehavior → Handler → Response
///
/// Nếu có lỗi validation → throw ValidationException ngay, Handler không chạy.
/// ExceptionMiddleware ở API layer sẽ bắt exception này và trả 400 Bad Request.
/// </summary>
public class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
  private readonly IEnumerable<IValidator<TRequest>> _validators;

  // DI inject tất cả Validator tương ứng với TRequest
  // Ví dụ: TRequest = RegisterCommand → inject RegisterCommandValidator
  public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
  {
    _validators = validators;
  }

  public async Task<TResponse> Handle(
      TRequest request,
      RequestHandlerDelegate<TResponse> next,
      CancellationToken cancellationToken)
  {
    // Không có validator nào được đăng ký → bỏ qua, vào Handler luôn
    if (!_validators.Any())
      return await next();

    // Chạy tất cả validators song song
    var context = new ValidationContext<TRequest>(request);

    var failures = _validators
        .Select(v => v.Validate(context))
        .SelectMany(result => result.Errors)
        .Where(failure => failure != null)
        .ToList();

    // Có lỗi → throw, Handler không được gọi
    if (failures.Count != 0)
      throw new ValidationException(failures);

    // Validation pass → tiếp tục vào Handler
    return await next();
  }
}