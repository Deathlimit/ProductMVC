using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ProductMVC.DAL;
using ProductMVC.DAL.Entities;
using ProductMVC.DAL.Repositories;
using Xunit;

namespace ProductMVC.Tests.Repositories;

public class ProductRepositoryTests
{
    [Fact(DisplayName = "Update -> Preserves CreatedAt")]
    public async Task Update_PreservesCreatedAt()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new ProductRepository(context);
        var createdAt = new DateTime(2026, 1, 1, 1, 0, 0, DateTimeKind.Utc);

        context.Products.Add(new ProductEntity
        {
            Id = 1,
            Name = "Original",
            Description = "Original description",
            Category = "Category1",
            Price = 100,
            Quantity = 5,
            CreatedAt = createdAt,
            UpdatedAt = createdAt
        });
        await context.SaveChangesAsync();

        // Act
        await repository.UpdateAsync(new ProductEntity
        {
            Id = 1,
            Name = "Updated",
            Description = "Updated description",
            Category = "Category2",
            Price = 120,
            Quantity = 7
        });

        // Assert
        var updated = await context.Products.SingleAsync(product => product.Id == 1);
        updated.CreatedAt.Should().Be(createdAt);
        updated.Name.Should().Be("Updated");
        updated.Description.Should().Be("Updated description");
        updated.Category.Should().Be("Category2");
        updated.Price.Should().Be(120);
        updated.Quantity.Should().Be(7);
        updated.UpdatedAt.Should().BeAfter(createdAt);
    }

    [Fact(DisplayName = "Update nonexistent -> KeyNotFoundException")]
    public async Task Update_NotFound()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new ProductRepository(context);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => repository.UpdateAsync(new ProductEntity
        {
            Id = 9999,
            Name = "Ghost",
            Price = 100,
            Quantity = 10
        }));
    }

    [Fact(DisplayName = "BatchUpdate -> Preserves optional fields")]
    public async Task BatchUpdate_PreservesOptionalFields()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new ProductRepository(context);
        var createdAt = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        context.Products.Add(new ProductEntity
        {
            Id = 2,
            Name = "Original",
            Description = "Keep me",
            Category = "Category1",
            Price = 100,
            Quantity = 5,
            CreatedAt = createdAt,
            UpdatedAt = createdAt
        });
        await context.SaveChangesAsync();

        // Act
        await repository.BatchUpdateAsync(new List<ProductEntity>
        {
            new()
            {
                Id = 2,
                Name = "Updated",
                Description = null,
                Category = null,
                Price = 130,
                Quantity = 9
            }
        });

        // Assert
        var updated = await context.Products.SingleAsync(product => product.Id == 2);
        updated.CreatedAt.Should().Be(createdAt);
        updated.Description.Should().Be("Keep me");
        updated.Category.Should().Be("Category1");
        updated.Name.Should().Be("Updated");
        updated.Price.Should().Be(130);
        updated.Quantity.Should().Be(9);
    }

    [Fact(DisplayName = "BatchUpdate missing IDs -> KeyNotFoundException")]
    public async Task BatchUpdate_MissingIds()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new ProductRepository(context);

        context.Products.Add(new ProductEntity
        {
            Id = 1,
            Name = "Exists",
            Price = 100,
            Quantity = 10
        });
        await context.SaveChangesAsync();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => repository.BatchUpdateAsync(new List<ProductEntity>
        {
            new() { Id = 1, Name = "OK", Price = 100, Quantity = 10 },
            new() { Id = 9999, Name = "Ghost", Price = 100, Quantity = 10 }
        }));
        
        ex.Message.Should().Contain("9999");
    }

    [Fact(DisplayName = "BatchInsert -> Sets timestamps")]
    public async Task BatchInsert_SetsTimestamps()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new ProductRepository(context);

        var beforeInsert = DateTime.UtcNow;

        // Act
        await repository.BatchInsertAsync(new List<ProductEntity>
        {
            new() { Name = "Product 1", Price = 100m, Quantity = 5 },
            new() { Name = "Product 2", Price = 200m, Quantity = 10 },
            new() { Name = "Product 3", Price = 300m, Quantity = 15 }
        });

        var afterInsert = DateTime.UtcNow;

        // Assert
        var products = await context.Products.ToListAsync();
        products.Should().HaveCount(3);
        products.All(p => p.CreatedAt >= beforeInsert && p.CreatedAt <= afterInsert).Should().BeTrue();
        products.All(p => p.UpdatedAt >= beforeInsert && p.UpdatedAt <= afterInsert).Should().BeTrue();
    }

    [Fact(DisplayName = "BatchDelete -> Deletes products")]
    public async Task BatchDelete_DeletesProducts()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new ProductRepository(context);

        context.Products.AddRange(
            new ProductEntity { Id = 1, Name = "DeleteMe 1", Price = 100, Quantity = 10 },
            new ProductEntity { Id = 2, Name = "DeleteMe 2", Price = 200, Quantity = 20 },
            new ProductEntity { Id = 3, Name = "Still here", Price = 300, Quantity = 30 }
        );
        await context.SaveChangesAsync();

        // Act
        await repository.BatchDeleteAsync(new List<int> { 1, 2 });

        // Assert
        var remaining = await context.Products.ToListAsync();
        remaining.Should().HaveCount(1);
        remaining.Single().Id.Should().Be(3);
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}