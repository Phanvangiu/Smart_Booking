using Microsoft.EntityFrameworkCore;
using SmartBooking.Domain.Entities;

namespace SmartBooking.Infrastructure.Persistence
{
  public class AppDbContext : DbContext
  {
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      base.OnModelCreating(modelBuilder);
      // Áp dụng tất cả Configuration trong assembly này
      // Thay vì gọi từng cái: modelBuilder.ApplyConfiguration(new UserConfiguration())
      // Dùng scan tự động — thêm file mới không cần sửa chỗ này

      modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppContext).Assembly);


      // Seed dữ liệu Role mặc định
      // Phải có trước khi Register được gọi
      SeedRoles(modelBuilder);
    }

    private static void SeedRoles(ModelBuilder modelBuilder)
    {
      var seedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc); // cố định
      modelBuilder.Entity<Role>().HasData(
          new Role
          {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Name = "Admin",
            Description = "Quản trị viên hệ thống",
            CreatedAt = seedDate
          },
          new Role
          {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Name = "BusinessOwner",
            Description = "Chủ doanh nghiệp",
            CreatedAt = seedDate
          },
          new Role
          {
            Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            Name = "Staff",
            Description = "Nhân viên",
            CreatedAt = seedDate
          },
          new Role
          {
            Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
            Name = "Customer",
            Description = "Khách hàng",
            CreatedAt = seedDate
          }
      );
    }
  }

}