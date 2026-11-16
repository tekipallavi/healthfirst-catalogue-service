using CatalogueService.Domain;

namespace CatalogueService.Api.Products;

/// <summary>Contains the product details returned by the API.</summary>
public sealed record ProductResponse(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    ProductStatus Status,
    DateTimeOffset CreatedAt);
