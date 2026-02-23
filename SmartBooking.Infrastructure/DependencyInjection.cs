using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartBooking.Application.Interfaces;
using SmartBooking.Infrastructure.Persistence;
using SmartBooking.Infrastructure.Repositories;
using SmartBooking.Infrastructure.Services;

namespace SmartBooking.Infrastructure;

/// <summary>
/// Đăng ký tất cả services của Infrastructure layer vào DI Container.
/// Được gọi 1 lần duy nhất trong Program.cs:
///     builder.Services.AddInfrastructureServices(builder.Configuration);
///
/// API layer chỉ biết gọi method này.
/// Không biết bên trong dùng EF Core, BCrypt, hay JWT — đúng Clean Architecture.
/// </summary>
public static class DependencyInjection
{
  public static IServiceCollection AddInfrastructureServices(
      this IServiceCollection services,
      IConfiguration configuration)
  {
    // --- Database ---
    services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(
            configuration.GetConnectionString("DefaultConnection"),
            sqlOptions =>
            {
              // Tự động retry khi DB tạm thời không kết nối được
              sqlOptions.EnableRetryOnFailure(
                      maxRetryCount: 5,
                      maxRetryDelay: TimeSpan.FromSeconds(30),
                      errorNumbersToAdd: null);
            }));

    // --- Repositories & UnitOfWork ---
    // Scoped: tạo mới 1 lần per HTTP request, dispose khi request kết thúc
    // Đảm bảo cùng 1 DbContext instance trong suốt 1 request
    services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
    services.AddScoped<IUnitOfWork, UnitOfWork>();

    // --- Services ---
    // Transient: tạo mới mỗi lần inject — phù hợp vì stateless
    services.AddTransient<IPasswordService, PasswordService>();
    services.AddTransient<ITokenService, TokenService>();

    // EmailService thêm sau khi setup SendGrid (Phase 4)
    // services.AddTransient<IEmailService, EmailService>();

    return services;
  }
}