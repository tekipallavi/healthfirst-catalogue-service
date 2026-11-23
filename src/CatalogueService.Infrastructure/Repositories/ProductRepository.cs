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
        query = sortOrder == "asc"
? query.OrderBy(p => p.Name)
: query.OrderByDescending(p => p.Name);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        await dbContext.Products.AddAsync(product, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}