using SmartBooking.Domain.Entities;

namespace SmartBooking.Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
  IRepository<User> Users { get; }
  IRepository<Role> Roles { get; }
  // Sau này thêm: Bookings, Services, Payments...

  Task<int> SaveChangesAsync(CancellationToken ct = default);
}