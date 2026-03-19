using FluentAssertions;
using FluentValidation;
using Moq;
using ProductMVC.BLL.DTO;
using ProductMVC.BLL.Services;
using ProductMVC.DAL.Entities;
using ProductMVC.DAL.Interfaces;
using Xunit;

namespace ProductMVC.Tests.Services;


public class ProductServiceBatchUpdateTests
{
    private readonly Mock<IProductRepository> _repositoryMock;
    private readonly Mock<IValidator<ProductDTO>> _validatorMock;
    private readonly ProductService _productService;

    public ProductServiceBatchUpdateTests()
    {
        _repositoryMock = new Mock<IProductRepository>();
        _validatorMock = new Mock<IValidator<ProductDTO>>();
        _productService = new ProductService(_repositoryMock.Object, _validatorMock.Object);
    }

    [Fact(DisplayName = "Update valid products → BatchUpdateAsync called")]
    public async Task Update_ValidProducts()
    {
        // Arrange
        var products = new List<ProductDTO>
        {
            new() { Id = 1, Name = "Updated Laptop", Price = 999.99m, Quantity = 8 },
            new() { Id = 2, Name = "Updated Mouse", Price = 24.99m, Quantity = 45 }
        };

        _validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<ProductDTO>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        // Act
        await _productService.BulkUpdateProducts(products);

        // Assert
        _repositoryMock.Verify(r => r.BatchUpdateAsync(It.Is<List<ProductEntity>>(l => l.Count == 2)), Times.Once);
    }

    [Fact(DisplayName = "Update invalid ID → ValidationException")]
    public async Task Update_InvalidId()
    {
        // Arrange
        var products = new List<ProductDTO>
        {
            new() { Id = 0, Name = "Invalid", Price = 100m, Quantity = 10 }
        };

        _validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<ProductDTO>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _productService.BulkUpdateProducts(products));
        _repositoryMock.Verify(r => r.BatchUpdateAsync(It.IsAny<List<ProductEntity>>()), Times.Never);
    }

    [Fact(DisplayName = "Update negative quantity → ValidationException")]
    public async Task Update_NegativeQuantity()
    {
        // Arrange
        var products = new List<ProductDTO>
        {
            new() { Id = 1, Name = "Invalid", Price = 100m, Quantity = -5 }
        };

        _validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<ProductDTO>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult(new[]
            {
                new FluentValidation.Results.ValidationFailure("Quantity", "Product quantity cannot be negative")
            }));

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _productService.BulkUpdateProducts(products));
        _repositoryMock.Verify(r => r.BatchUpdateAsync(It.IsAny<List<ProductEntity>>()), Times.Never);
    }
}
