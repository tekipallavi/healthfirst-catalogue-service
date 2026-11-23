using CatalogueService.Domain;

namespace CatalogueService.Application.Products;

public sealed class ProductService(IProductRepository productRepository) : IProductService
{
    
    public async Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await productRepository.GetByIdAsync(id, cancellationToken);
        return product is null ? null : ToDto(product);
    }

    public async Task<IReadOnlyList<ProductDto>> GetAllAsync(string search, string sortBy, string sortOrder, CancellationToken cancellationToken = default)
    {

        var products = await productRepository.GetAllAsync(search, sortBy, sortOrder, cancellationToken);
        return products.Select(ToDto).ToArray();
    }

    public async Task<ProductDto> CreateAsync(
        CreateProductCommand command,
        CancellationToken cancellationToken = default)
    {
        var product = new Product(command.Name, command.Description, command.Price, command.Status);
        await productRepository.AddAsync(product, cancellationToken);
        return ToDto(product);
    }

    private static ProductDto ToDto(Product product) =>
        new(
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            product.Status,
            new DateTimeOffset(product.CreatedAt, TimeSpan.Zero));
}
