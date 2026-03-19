using FluentAssertions;
using FluentValidation;
using Moq;
using ProductMVC.BLL.DTO;
using ProductMVC.BLL.Services;
using ProductMVC.DAL.Entities;
using ProductMVC.DAL.Interfaces;
using Xunit;

namespace ProductMVC.Tests.Services;


public class ProductServiceBatchInsertTests
{
    private readonly Mock<IProductRepository> _repositoryMock;
    private readonly Mock<IValidator<ProductDTO>> _validatorMock;
    private readonly ProductService _productService;

    public ProductServiceBatchInsertTests()
    {
        _repositoryMock = new Mock<IProductRepository>();
        _validatorMock = new Mock<IValidator<ProductDTO>>();
        _productService = new ProductService(_repositoryMock.Object, _validatorMock.Object);
    }

    [Fact(DisplayName = "Insert valid products → BatchInsertAsync called")]
    public async Task Insert_ValidProducts()
    {
        // Arrange
        var products = new List<ProductDTO>
        {
            new() { Name = "Laptop", Price = 1299.99m, Quantity = 5 },
            new() { Name = "Mouse", Price = 29.99m, Quantity = 50 },
            new() { Name = "Keyboard", Price = 149.99m, Quantity = 25 }
        };

        _validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<ProductDTO>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        // Act
        await _productService.BulkInsertProducts(products);

        // Assert
        _repositoryMock.Verify(r => r.BatchInsertAsync(It.Is<List<ProductEntity>>(l => l.Count == 3)), Times.Once);
    }

    [Fact(DisplayName = "Insert invalid price → ValidationException")]
    public async Task Insert_InvalidPrice()
    {
        // Arrange
        var products = new List<ProductDTO>
        {
            new() { Name = "Invalid", Price = 0m, Quantity = 10 }
        };

        _validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<ProductDTO>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult(new[]
            {
                new FluentValidation.Results.ValidationFailure("Price", "Price must be greater than 0")
            }));

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _productService.BulkInsertProducts(products));
        _repositoryMock.Verify(r => r.BatchInsertAsync(It.IsAny<List<ProductEntity>>()), Times.Never);
    }

    [Fact(DisplayName = "Insert empty name → ValidationException")]
    public async Task Insert_EmptyName()
    {
        // Arrange
        var products = new List<ProductDTO>
        {
            new() { Name = "", Price = 100m, Quantity = 10 }
        };

        _validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<ProductDTO>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult(new[]
            {
                new FluentValidation.Results.ValidationFailure("Name", "Product name is required")
            }));

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _productService.BulkInsertProducts(products));
        _repositoryMock.Verify(r => r.BatchInsertAsync(It.IsAny<List<ProductEntity>>()), Times.Never);
    }
}
