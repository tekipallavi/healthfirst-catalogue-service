using CatalogueService.Application.Products;
using CatalogueService.Domain;
using CatalogueService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CatalogueService.Infrastructure.Repositories;

public sealed class ProductRepository(CatalogueDbContext dbContext) : IProductRepository
{
    public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Product>> GetAllAsync(string search, string sortBy, string sortOrder, CancellationToken cancellationToken = default)

    {
        var products = dbContext.Products.AsNoTracking();
        var query = products.Where(p => p.Name.Contains(search) || p.Description.Contains(search));
        query = (sortBy, sortOrder) switch
        {
            ("name", "asc") => query.OrderBy(p => p.Name),
            ("name", "desc") => query.OrderByDescending(p => p.Name),
            ("price", "asc") => query.OrderBy(p => p.Price),
            ("price", "desc") => query.OrderByDescending(p => p.Price),
            ("status", "asc") => query.OrderBy(p => p.Status),
            ("status", "desc") => query.OrderByDescending(p => p.Status),
            ("createdAt", "asc") => query.OrderBy(p => p.CreatedAt),
            ("createdAt", "desc") => query.OrderByDescending(p => p.CreatedAt),
            _ => throw new ArgumentException(
                $"Unsupported sort field '{sortBy}' or sort order '{sortOrder}'.")
        };

        return await query.ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        await dbContext.Products.AddAsync(product, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}