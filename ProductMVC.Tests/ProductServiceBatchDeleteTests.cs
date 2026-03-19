using FluentAssertions;
using FluentValidation;
using Moq;
using ProductMVC.BLL.DTO;
using ProductMVC.BLL.Services;
using ProductMVC.DAL.Interfaces;
using Xunit;

namespace ProductMVC.Tests.Services;


public class ProductServiceBatchDeleteTests
{
    private readonly Mock<IProductRepository> _repositoryMock;
    private readonly Mock<IValidator<ProductDTO>> _validatorMock;
    private readonly ProductService _productService;

    public ProductServiceBatchDeleteTests()
    {
        _repositoryMock = new Mock<IProductRepository>();
        _validatorMock = new Mock<IValidator<ProductDTO>>();
        _productService = new ProductService(_repositoryMock.Object, _validatorMock.Object);
    }

    [Fact(DisplayName = "Delete valid IDs → BatchDeleteAsync called")]
    public async Task Delete_ValidIds()
    {
        // Arrange
        var productIds = new List<int> { 1, 2, 3 };

        // Act
        await _productService.BulkDeleteProducts(productIds);

        // Assert
        _repositoryMock.Verify(r => r.BatchDeleteAsync(productIds), Times.Once);
    }

    [Fact(DisplayName = "Delete invalid ID → ValidationException")]
    public async Task Delete_InvalidId()
    {
        // Arrange
        var productIds = new List<int> { 1, 0, 3 };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _productService.BulkDeleteProducts(productIds));
        _repositoryMock.Verify(r => r.BatchDeleteAsync(It.IsAny<List<int>>()), Times.Never);
    }

    [Fact(DisplayName = "Delete negative IDs → ValidationException")]
    public async Task Delete_NegativeIds()
    {
        // Arrange
        var productIds = new List<int> { -1, -2, -3 };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _productService.BulkDeleteProducts(productIds));
        _repositoryMock.Verify(r => r.BatchDeleteAsync(It.IsAny<List<int>>()), Times.Never);
    }

    [Fact(DisplayName = "Delete single ID → BatchDeleteAsync called")]
    public async Task Delete_SingleId()
    {
        // Arrange
        var productIds = new List<int> { 42 };

        // Act
        await _productService.BulkDeleteProducts(productIds);

        // Assert
        _repositoryMock.Verify(r => r.BatchDeleteAsync(productIds), Times.Once);
    }
}
