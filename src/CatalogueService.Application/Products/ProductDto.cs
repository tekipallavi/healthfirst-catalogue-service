using CatalogueService.Domain;

namespace CatalogueService.Application.Products;

public sealed record ProductDto(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    int? Quantity,
    ProductStatus Status,
    DateTimeOffset CreatedAt);
