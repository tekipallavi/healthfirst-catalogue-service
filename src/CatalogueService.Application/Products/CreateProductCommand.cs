using CatalogueService.Domain;

namespace CatalogueService.Application.Products;

public sealed record CreateProductCommand(
    string Name,
    string Description,
    decimal Price,
    ProductStatus Status,
    int Quantity = 0);
 