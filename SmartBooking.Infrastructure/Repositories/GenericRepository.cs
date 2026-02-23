using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SmartBooking.Application.Interfaces;
using SmartBooking.Infrastructure.Persistence;

namespace SmartBooking.Infrastructure.Repositories
{

  /// <summary>
  /// Implement IRepository<T> bằng EF Core.
  ///
  /// Application layer hoàn toàn không biết class này tồn tại.
  /// Application chỉ gọi qua interface IRepository<T>.
  ///
  /// Generic T: dùng được cho mọi Entity (User, Role, Booking...)
  /// mà không cần viết lại logic CRUD.
  /// </summary>
  public class GenericRepository<T> : IRepository<T> where T : class
  {
    private readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;
    public GenericRepository(AppDbContext context)
    {
      _context = context;
      _dbSet = _context.Set<T>();
    }

    public async Task AddAsync(T entity, CancellationToken ct = default)
        => await _dbSet.AddAsync(entity, ct);

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
    => await _dbSet.FindAsync(new object[] { id }, ct);

    public async Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default)
      => await _dbSet.ToListAsync(ct);

    public void Update(T entity)
     => _dbSet.Update(entity);
    // EF Core track change trong memory — không cần async
    // SaveChangesAsync() ở UnitOfWork mới thực sự ghi xuống DB
    public void Delete(T entity)
        => _dbSet.Remove(entity);

    public async Task<bool> ExistsAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken ct = default)
        => await _dbSet.AnyAsync(predicate, ct);

    public async Task<T?> FirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken ct = default)
        => await _dbSet.FirstOrDefaultAsync(predicate, ct);
  }

}