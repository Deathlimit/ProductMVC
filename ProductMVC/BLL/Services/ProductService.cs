using FluentValidation;
using FluentValidation.Results;
using ProductMVC.BLL.DTO;
using ProductMVC.BLL.Interfaces;
using ProductMVC.BLL.Validation;
using ProductMVC.DAL.Entities;
using ProductMVC.DAL.Interfaces;

namespace ProductMVC.BLL.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IValidator<ProductDTO> _productValidator;
    private const int MaxPageSize = 100;

    public ProductService(
        IProductRepository productRepository,
        IValidator<ProductDTO> productValidator)
    {
        _productRepository = productRepository;
        _productValidator = productValidator;
    }

    public async Task<PagedResult<ProductDTO>> GetProductsAsync(int page = 1, int pageSize = 10)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

        var (items, totalCount) = await _productRepository.GetPagedAsync(page, pageSize);
        return new PagedResult<ProductDTO>
        {
            Items = items.Select(MapToDto).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<ProductDTO?> GetProductByIdAsync(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        return product != null ? MapToDto(product) : null;
    }

    public async Task BulkInsertProducts(List<ProductDTO> products)
    {
        await ValidateProductsAsync(products);
        var entities = products.Select(MapToEntity).ToList();
        await _productRepository.BatchInsertAsync(entities);
    }

    public async Task BulkUpdateProducts(List<ProductDTO> products)
    {
        await ValidateProductsAsync(products);
        ValidateProductIds(products.Select(product => product.Id).ToList());
        var entities = products.Select(MapToEntity).ToList();
        await _productRepository.BatchUpdateAsync(entities);
    }

    public async Task BulkDeleteProducts(List<int> productIds)
    {
        ValidateProductIds(productIds);
        await _productRepository.BatchDeleteAsync(productIds);
    }

    public async Task<ProductDTO> CreateProductAsync(ProductDTO productDto)
    {
        await _productValidator.ValidateAndThrowAsync(productDto);
        var entity = MapToEntity(productDto);
        await _productRepository.AddAsync(entity);
        return MapToDto(entity);
    }

    public async Task<ProductDTO> UpdateProductAsync(ProductDTO productDto)
    {
        await _productValidator.ValidateAndThrowAsync(productDto);
        ValidateProductIds(new List<int> { productDto.Id });
        var entity = MapToEntity(productDto);
        await _productRepository.UpdateAsync(entity);
        return MapToDto(entity);
    }

    public async Task DeleteProductAsync(int id)
    {
        await _productRepository.DeleteAsync(id);
    }

    private async Task ValidateProductsAsync(List<ProductDTO> products)
    {
        var validationResults = new List<ValidationFailure>();

        foreach (var product in products)
        {
            var result = await _productValidator.ValidateAsync(product);
            if (!result.IsValid)
            {
                validationResults.AddRange(result.Errors);
            }
        }

        if (validationResults.Any())
        {
            throw new ValidationException("Product validation failed", validationResults);
        }
    }

    private static void ValidateProductIds(List<int> productIds)
    {
        if (productIds.Any(id => id <= 0))
        {
            throw new ValidationException("Product validation failed", new[]
            {
                new ValidationFailure(nameof(ProductDTO.Id), "Product id must be greater than 0")
            });
        }
    }

    private static ProductDTO MapToDto(ProductEntity entity)
    {
        return new ProductDTO
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            Price = entity.Price,
            Quantity = entity.Quantity,
            Category = entity.Category
        };
    }

    private static ProductEntity MapToEntity(ProductDTO dto)
    {
        return new ProductEntity
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            Quantity = dto.Quantity,
            Category = dto.Category
        };
    }
}
