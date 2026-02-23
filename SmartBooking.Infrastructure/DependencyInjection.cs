using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartBooking.Application.Interfaces;
using SmartBooking.Infrastructure.Persistence;
using SmartBooking.Infrastructure.Repositories;
using SmartBooking.Infrastructure.Services;

namespace SmartBooking.Infrastructure;

public static class DependencyInjection
{
  public static IServiceCollection AddInfrastructureServices(
      this IServiceCollection services,
      IConfiguration configuration)
  {
    // --- Database ---
    var connectionString = configuration.GetConnectionString("DefaultConnection")!;

    // ServerVersion.AutoDetect kết nối 1 lần để detect version MySQL
    // Dùng khi biết chắc DB đang chạy lúc app start
    var serverVersion = new MySqlServerVersion(new Version(8, 0, 0));

    services.AddDbContext<AppDbContext>(options =>
        options.UseMySql(
            connectionString,
            serverVersion,
            mySqlOptions =>
            {
              mySqlOptions.EnableRetryOnFailure(
                      maxRetryCount: 5,
                      maxRetryDelay: TimeSpan.FromSeconds(30),
                      errorNumbersToAdd: null);
            }));

    // --- Repositories & UnitOfWork ---
    services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
    services.AddScoped<IUnitOfWork, UnitOfWork>();

    // --- Services ---
    services.AddTransient<IPasswordService, PasswordService>();
    services.AddTransient<ITokenService, TokenService>();

    return services;
  }
}