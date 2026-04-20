using BarberShop.Data;
using BarberShop.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BarberShop.Repositories;

public class EfRepository<TEntity>(BarberShopDbContext context) : IRepository<TEntity> where TEntity : class
{
    private readonly DbSet<TEntity> _dbSet = context.Set<TEntity>();

    public Task<List<TEntity>> GetAllAsync()
        => _dbSet.ToListAsync();

    public async Task<TEntity?> GetByIdAsync(int id)
        => await _dbSet.FindAsync(id);

    public Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)
        => _dbSet.FirstOrDefaultAsync(predicate);

    public Task<List<TEntity>> WhereAsync(Expression<Func<TEntity, bool>> predicate)
        => _dbSet.Where(predicate).ToListAsync();

    public async Task<TEntity> AddAsync(TEntity entity)
    {
        _dbSet.Add(entity);
        await context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(TEntity entity)
    {
        _dbSet.Update(entity);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(TEntity entity)
    {
        _dbSet.Remove(entity);
        await context.SaveChangesAsync();
    }
}
