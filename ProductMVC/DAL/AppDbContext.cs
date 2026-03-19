using Microsoft.EntityFrameworkCore;
using ProductMVC.DAL.Entities;

namespace ProductMVC.DAL;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<ProductEntity> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ProductEntity>()
            .HasKey(p => p.Id);

        modelBuilder.Entity<ProductEntity>()
            .Property(p => p.Name)
            .IsRequired();

        modelBuilder.Entity<ProductEntity>()
            .Property(p => p.Price)
            .HasPrecision(10, 2);
    }
}
