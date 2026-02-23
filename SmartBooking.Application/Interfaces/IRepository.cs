using System.Linq.Expressions;

namespace SmartBooking.Application.Interfaces;

public interface IRepository<T> where T : class
{
  // Query
  Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
  Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default);

  // Command
  Task AddAsync(T entity, CancellationToken ct = default);
  void Update(T entity);   // EF track thay đổi, không cần async
  void Delete(T entity);

  // Helper hay dùng
  Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
  Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
}