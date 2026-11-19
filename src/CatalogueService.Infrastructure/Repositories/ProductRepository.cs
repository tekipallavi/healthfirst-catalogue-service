using CatalogueService.Application.Products;
using CatalogueService.Domain;
using CatalogueService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CatalogueService.Infrastructure.Repositories;

public sealed class ProductRepository(CatalogueDbContext dbContext) : IProductRepository
{
    public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default) =>
        //await dbContext.Products.AsNoTracking().OrderBy(x => x.Name).ToListAsync(cancellationToken);
        //mock the data for now
        await Task.FromResult(new List<Product>
        {
            new ( "Product 1", "Description 1", 10, ProductStatus.Active),
            new ( "Product 2", "Description 2", 20, ProductStatus.Active),
            new ("Product 3", "Description 3", 30, ProductStatus.Active),
        } as IReadOnlyList<Product>);

    public async Task AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        await dbContext.Products.AddAsync(product, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}