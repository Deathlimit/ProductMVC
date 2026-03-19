using Microsoft.EntityFrameworkCore;
using ProductMVC.DAL.Entities;
using ProductMVC.DAL.Interfaces;

namespace ProductMVC.DAL.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;

    public ProductRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ProductEntity>> GetAllAsync()
    {
        return await _context.Products.ToListAsync();
    }

    public async Task<ProductEntity?> GetByIdAsync(int id)
    {
        return await _context.Products.FindAsync(id);
    }

    public async Task<(List<ProductEntity> Items, int TotalCount)> GetPagedAsync(int page, int pageSize)
    {
        var totalCount = await _context.Products.CountAsync();
        var items = await _context.Products
            .OrderByDescending(p => p.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task AddAsync(ProductEntity entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.Products.AddAsync(entity);
        await SaveChangesAsync();
    }

    public async Task UpdateAsync(ProductEntity entity)
    {
        var existingEntity = await _context.Products.FindAsync(entity.Id);
        if (existingEntity == null)
        {
            throw new KeyNotFoundException($"Product with id {entity.Id} was not found");
        }

        ApplyUpdates(existingEntity, entity, preserveOptionalFieldsWhenNull: false);
        await SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _context.Products.Remove(entity);
            await SaveChangesAsync();
        }
    }

    public async Task BatchInsertAsync(List<ProductEntity> entities)
    {
        foreach (var entity in entities)
        {
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
        }

        await _context.Products.AddRangeAsync(entities);
        await SaveChangesAsync();
    }

    public async Task BatchUpdateAsync(List<ProductEntity> entities)
    {
        var entityIds = entities.Select(entity => entity.Id).Distinct().ToList();
        var existingEntities = await _context.Products
            .Where(product => entityIds.Contains(product.Id))
            .ToDictionaryAsync(product => product.Id);

        var missingIds = entityIds
            .Where(id => !existingEntities.ContainsKey(id))
            .ToList();

        if (missingIds.Count != 0)
        {
            throw new KeyNotFoundException($"Products not found: {string.Join(", ", missingIds)}");
        }

        foreach (var entity in entities)
        {
            ApplyUpdates(existingEntities[entity.Id], entity, preserveOptionalFieldsWhenNull: true);
        }

        await SaveChangesAsync();
    }

    public async Task BatchDeleteAsync(List<int> ids)
    {
        var entities = await _context.Products
            .Where(p => ids.Contains(p.Id))
            .ToListAsync();

        _context.Products.RemoveRange(entities);
        await SaveChangesAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    private static void ApplyUpdates(ProductEntity target, ProductEntity source, bool preserveOptionalFieldsWhenNull)
    {
        target.Name = source.Name;
        target.Price = source.Price;
        target.Quantity = source.Quantity;
        target.Description = preserveOptionalFieldsWhenNull && source.Description == null
            ? target.Description
            : source.Description;
        target.Category = preserveOptionalFieldsWhenNull && source.Category == null
            ? target.Category
            : source.Category;
        target.UpdatedAt = DateTime.UtcNow;
    }
}
