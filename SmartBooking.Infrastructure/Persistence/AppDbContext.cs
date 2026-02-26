using Microsoft.EntityFrameworkCore;
using SmartBooking.Domain.Entities;

namespace SmartBooking.Infrastructure.Persistence
{
  public class AppDbContext : DbContext
  {
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }


    public DbSet<Business> Businesses { get; set; }
    public DbSet<BusinessRole> BusinessRoles { get; set; }
    public DbSet<BusinessUser> BusinessUsers { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<BusinessRolePermission> BusinessRolePermissions { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      base.OnModelCreating(modelBuilder);
      // Áp dụng tất cả Configuration trong assembly này
      // Thay vì gọi từng cái: modelBuilder.ApplyConfiguration(new UserConfiguration())
      // Dùng scan tự động tìm tất cả class implement IEntityTypeConfiguration<T> và áp dụng

      modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppContext).Assembly);


      // Seed dữ liệu Role mặc định
      // Phải có trước khi Register được gọi
    }


  }

}