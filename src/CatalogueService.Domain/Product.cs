namespace CatalogueService.Domain;

public sealed class Product
{
    private Product()
    {
    }

    public Product(string name, string description, decimal price, ProductStatus status = ProductStatus.Active, int? quantity = null)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        Price = price;
        Quantity = quantity;
        Status = status;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public ProductStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public int? Quantity { get; private set; }
}