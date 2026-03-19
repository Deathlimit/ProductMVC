using ProductMVC.DAL.Entities;

namespace ProductMVC.DAL.Interfaces;

public interface IProductRepository
{
    Task<IEnumerable<ProductEntity>> GetAllAsync();
    Task<ProductEntity?> GetByIdAsync(int id);
    Task<(List<ProductEntity> Items, int TotalCount)> GetPagedAsync(int page, int pageSize);
    Task AddAsync(ProductEntity entity);
    Task UpdateAsync(ProductEntity entity);
    Task DeleteAsync(int id);
    Task BatchInsertAsync(List<ProductEntity> entities);
    Task BatchUpdateAsync(List<ProductEntity> entities);
    Task BatchDeleteAsync(List<int> ids);
    Task SaveChangesAsync();
}
