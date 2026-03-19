using ProductMVC.BLL.DTO;

namespace ProductMVC.BLL.Interfaces;

public interface IProductService
{
    Task<PagedResult<ProductDTO>> GetProductsAsync(int page = 1, int pageSize = 10);
    Task<ProductDTO?> GetProductByIdAsync(int id);
    Task BulkInsertProducts(List<ProductDTO> products);
    Task BulkUpdateProducts(List<ProductDTO> products);
    Task BulkDeleteProducts(List<int> productIds);
    Task<ProductDTO> CreateProductAsync(ProductDTO productDto);
    Task<ProductDTO> UpdateProductAsync(ProductDTO productDto);
    Task DeleteProductAsync(int id);
}
