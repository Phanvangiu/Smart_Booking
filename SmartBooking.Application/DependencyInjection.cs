using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using SmartBooking.Application.Common;
using System.Reflection;

namespace SmartBooking.Application;

/// <summary>
/// Đăng ký tất cả services của Application layer vào DI Container.
/// Được gọi 1 lần duy nhất trong Program.cs:
///     builder.Services.AddApplicationServices();
/// </summary>
public static class DependencyInjection
{
  public static IServiceCollection AddApplicationServices(
      this IServiceCollection services)
  {
    var assembly = Assembly.GetExecutingAssembly();

    // MediatR: tự scan assembly, tìm và đăng ký tất cả IRequestHandler
    // → RegisterCommandHandler, LoginCommandHandler, v.v.
    services.AddMediatR(cfg =>
        cfg.RegisterServicesFromAssembly(assembly));

    // FluentValidation: tự scan, tìm và đăng ký tất cả AbstractValidator
    // → RegisterCommandValidator, LoginCommandValidator, v.v.
    services.AddValidatorsFromAssembly(assembly);

    // Pipeline Behavior: chạy ValidationBehavior TRƯỚC mỗi Handler
    // typeof(IPipelineBehavior<,>) → đăng ký cho mọi cặp TRequest/TResponse
    services.AddTransient(
        typeof(IPipelineBehavior<,>),
        typeof(ValidationBehavior<,>));

    return services;
  }
}