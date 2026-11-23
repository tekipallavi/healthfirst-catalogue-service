namespace CatalogueService.Application.Products;

public interface IProductService
{
    Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductDto>> GetAllAsync(string search, string sortBy, string sortOrder, CancellationToken cancellationToken = default);
    Task<ProductDto> CreateAsync(CreateProductCommand command, CancellationToken cancellationToken = default);
}
