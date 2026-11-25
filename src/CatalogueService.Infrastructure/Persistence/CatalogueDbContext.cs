using CatalogueService.Domain;
using Microsoft.EntityFrameworkCore;

namespace CatalogueService.Infrastructure.Persistence;


public sealed class CatalogueDbContext(DbContextOptions<CatalogueDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var product = modelBuilder.Entity<Product>();

        product.HasKey(x => x.Id);
        product.Property(x => x.Name).HasMaxLength(200).IsRequired();
        product.Property(x => x.Description).HasMaxLength(1000).IsRequired();
        product.Property(x => x.Price).HasPrecision(18, 2);
        product.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        product.Property(x => x.Quantity).IsRequired(false);
        product.Property(x => x.CreatedAt).IsRequired();
        product.HasIndex(x => x.Name);
    }
}