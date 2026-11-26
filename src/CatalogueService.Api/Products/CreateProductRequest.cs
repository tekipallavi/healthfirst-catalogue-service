using System.ComponentModel.DataAnnotations;
using CatalogueService.Domain;

namespace CatalogueService.Api.Products;

/// <summary>Contains the details needed to create a product.</summary>
public sealed record CreateProductRequest
{
    [Required, StringLength(200)]
    public required string Name { get; init; }

    [Required, StringLength(1000)]
    public required string Description { get; init; }

    [Range(typeof(decimal), "0.01", "9999999999999999.99")]
    public required decimal Price { get; init; }

    public int Quantity { get; init; }

    public ProductStatus Status { get; init; } = ProductStatus.Active;
}
