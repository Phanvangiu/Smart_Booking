using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SmartBooking.API.Middleware;
using SmartBooking.Application;
using SmartBooking.Infrastructure;
using SmartBooking.Infrastructure.Persistence;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// =========================================================
// 1. ĐĂNG KÝ SERVICES
// =========================================================
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
      // Enum → string ("Pending" thay vì 1)
      options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
      // Bỏ null fields trong response
      options.JsonSerializerOptions.DefaultIgnoreCondition =
          JsonIgnoreCondition.WhenWritingNull;
    });

// =========================================================
// 2. JWT AUTHENTICATION
// =========================================================
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"]!;

builder.Services.AddAuthentication(options =>
{
  options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
  options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
  options.TokenValidationParameters = new TokenValidationParameters
  {
    ValidateIssuer = true,
    ValidateAudience = true,
    ValidateLifetime = true,
    ValidateIssuerSigningKey = true,
    ValidIssuer = jwtSettings["Issuer"],
    ValidAudience = jwtSettings["Audience"],
    IssuerSigningKey = new SymmetricSecurityKey(
          Encoding.UTF8.GetBytes(secretKey)),
    ClockSkew = TimeSpan.Zero
  };

  // 401 trả về JSON thay vì HTML
  options.Events = new JwtBearerEvents
  {
    OnChallenge = async context =>
    {
      context.HandleResponse();
      context.Response.StatusCode = 401;
      context.Response.ContentType = "application/json";
      await context.Response.WriteAsync(
              """{"isSuccess":false,"errors":["Bạn chưa đăng nhập hoặc token không hợp lệ"]}""");
    }
  };
});

builder.Services.AddAuthorization();

// =========================================================
// 3. CORS
// =========================================================
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
  options.AddPolicy("AllowFrontend", policy =>
      policy
          .WithOrigins(allowedOrigins)
          .AllowAnyMethod()
          .AllowAnyHeader()
          .AllowCredentials());
});

// =========================================================
// 4. SWAGGER — tương thích Swashbuckle 10.x
//
// Swashbuckle 10 đổi cách khai báo security:
// KHÔNG dùng: new OpenApiSecurityScheme { Reference = ... }
// PHẢI dùng:  SecuritySchemeId (string key) trong AddSecurityRequirement
// =========================================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
  options.SwaggerDoc("v1", new OpenApiInfo
  {
    Title = "SmartBooking API",
    Version = "v1",
    Description = "API cho hệ thống đặt lịch thông minh"
  });

  // Bước 1: Đăng ký scheme với key "Bearer"
  options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
  {
    Name = "Authorization",
    Type = SecuritySchemeType.Http,
    Scheme = "Bearer",
    BearerFormat = "JWT",
    In = ParameterLocation.Header,
    Description = "Nhập JWT token. Ví dụ: Bearer eyJhbGci..."
  });

  // Bước 2: Swashbuckle 10.x — dùng SecuritySchemeId thay vì Reference object
  options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Id = "Bearer", Type = ReferenceType.SecurityScheme } },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddSignalR();

// =========================================================
// 5. BUILD
// =========================================================
var app = builder.Build();

// =========================================================
// 6. MIDDLEWARE PIPELINE — THỨ TỰ QUAN TRỌNG
// =========================================================

// Ngoài cùng — bắt exception từ mọi layer phía trong
app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI(c =>
  {
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "SmartBooking API v1");
    c.RoutePrefix = string.Empty;
  });
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");

// Authentication trước Authorization — bắt buộc
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// =========================================================
// 7. AUTO MIGRATE (Development only)
// =========================================================
if (app.Environment.IsDevelopment())
{
  using var scope = app.Services.CreateScope();
  var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
  await db.Database.MigrateAsync();
}

app.Run();