using SmartBooking.Application.Interfaces;
using SmartBooking.Domain.Entities;
using SmartBooking.Infrastructure.Persistence;

namespace SmartBooking.Infrastructure.Repositories;

/// <summary>
/// Implement IUnitOfWork — quản lý tất cả Repository và transaction.
///
/// Tại sao cần UnitOfWork?
/// Giả sử 1 Handler cần:
///   1. Tạo User mới
///   2. Gửi email verification
/// Nếu bước 2 lỗi, bước 1 phải rollback.
/// UnitOfWork đảm bảo: tất cả hoặc không có gì được lưu — atomic.
///
/// Handler chỉ gọi SaveChangesAsync() 1 lần duy nhất ở cuối.
/// Tất cả thay đổi trước đó được EF Core track trong memory.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
  private readonly AppDbContext _context;

  // Lazy initialization — chỉ tạo Repository khi được dùng lần đầu
  private IRepository<User>? _users;
  private IRepository<Role>? _roles;

  public UnitOfWork(AppDbContext context)
  {
    _context = context;
  }

  public IRepository<User> Users
      => _users ??= new GenericRepository<User>(_context);

  public IRepository<Role> Roles
      => _roles ??= new GenericRepository<Role>(_context);

  // Sau này thêm khi build Phase 2:
  // private IRepository<Booking>? _bookings;
  // public IRepository<Booking> Bookings
  //     => _bookings ??= new GenericRepository<Booking>(_context);

  /// <summary>
  /// Commit tất cả thay đổi xuống database trong 1 transaction.
  /// Trả về số records bị ảnh hưởng.
  /// </summary>
  public async Task<int> SaveChangesAsync(CancellationToken ct = default)
      => await _context.SaveChangesAsync(ct);

  public void Dispose()
      => _context.Dispose();
}