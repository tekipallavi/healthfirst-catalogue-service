using CatalogueService.Application.Products;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CatalogueService.Api.Products;

public static class ProductEndpoints
{
    private static readonly string[] SortFields = [ "name", "price", "status", "createdAt" ]; 
    private static readonly string[] SortOrders = [ "asc", "desc" ];
    private static readonly (string Name, string Description, decimal Price, int Quantity)[] SeedProducts =
    [
        ("Digital Thermometer", "Fast-reading digital thermometer with a flexible tip.", 12.99m, 48),
        ("First Aid Kit", "Compact 100-piece first aid kit for home and travel.", 24.50m, 32),
        ("Vitamin D3 1000 IU", "Bottle of 120 vitamin D3 softgels.", 10.75m, 65),
        ("Reusable Hot and Cold Pack", "Flexible reusable pack for hot or cold therapy.", 8.99m, 40),
        ("Weekly Pill Organizer", "Seven-day pill organizer with separate daily compartments.", 6.49m, 55),
        ("Hypoallergenic Bandages", "Assorted latex-free adhesive bandages, pack of 50.", 5.25m, 80),
        ("Saline Nasal Spray", "Drug-free saline nasal spray for everyday moisture.", 7.99m, 38),
        ("Graduated Compression Socks", "Knee-high graduated compression socks for daily wear.", 18.00m, 27),
        ("Digital Blood Pressure Monitor", "Upper-arm blood pressure monitor with memory for two users.", 39.99m, 21),
        ("Pulse Oximeter", "Fingertip pulse oximeter with an easy-to-read display.", 22.50m, 34),
        ("Alcohol Prep Pads", "Sterile isopropyl alcohol prep pads, box of 100.", 4.75m, 90),
        ("Antibacterial Hand Sanitizer", "Travel-size 70 percent alcohol hand sanitizer, 8 fl oz.", 3.99m, 72),
        ("Gauze Pads", "Sterile 4 by 4 inch gauze pads, pack of 25.", 6.25m, 46),
        ("Elastic Support Bandage", "Washable elastic bandage with fastening clips.", 7.50m, 31),
        ("Hot Water Bottle", "Rubber hot water bottle with a soft protective cover.", 14.99m, 18),
        ("Foam Earplugs", "Soft disposable foam earplugs, pack of 20 pairs.", 5.99m, 60),
        ("Reusable Ice Bag", "Leak-resistant reusable ice bag for cold therapy.", 11.25m, 24),
        ("Microwaveable Heating Pad", "Microwaveable fabric heating pad for targeted warmth.", 19.50m, 16),
        ("Children's Liquid Bandages", "Flexible liquid bandage for minor cuts and scrapes.", 8.25m, 29),
        ("Oral Rehydration Salts", "Electrolyte powder sachets for oral rehydration, pack of 10.", 9.99m, 42),
        ("Lubricating Eye Drops", "Preservative-free lubricating eye drops, pack of 30 vials.", 13.75m, 26),
        ("Non-Contact Infrared Thermometer", "Forehead thermometer with quick readings and fever alert.", 32.00m, 19),
        ("Medical Face Masks", "Disposable three-layer medical face masks, pack of 50.", 12.50m, 58)
    ];

    public static WebApplication MapProducts(this WebApplication app)
    {
        var products = app.MapGroup("/api/products").WithTags("Products");

        products.MapGet("", async Task<Results<Ok<IReadOnlyList<ProductResponse>>, ValidationProblem> > (
            string? search,
            string? sortBy,
            string? sortOrder,
            IProductService productService,
            CancellationToken cancellationToken) =>
        {
            search = string.IsNullOrWhiteSpace(search)? "" : search.Trim();
            sortBy = string.IsNullOrWhiteSpace(sortBy)? "createdAt" : SortFields.FirstOrDefault(field => string.Equals(sortBy.Trim(), field, StringComparison.OrdinalIgnoreCase));
            sortOrder = string.IsNullOrWhiteSpace(sortOrder)? "asc" : SortOrders.FirstOrDefault(order => string.Equals(sortOrder.Trim(), order, StringComparison.OrdinalIgnoreCase));
          
            if(sortBy is null || sortOrder is null)
            {
                var errors = new Dictionary<string, string[]>();
                if(sortBy is null)
                {
                   errors["sortBy"] = ["SortBy should have only Name, Price, CreatedAt"];
                } 
                if(sortOrder is null)
                {
                   errors["sortOrder"] = ["SortBy should have only asc or desc"];
                } 
                return TypedResults.ValidationProblem(errors);
            }
            var result = await productService.GetAllAsync(search, sortBy, sortOrder, cancellationToken);
            return TypedResults.Ok<IReadOnlyList<ProductResponse>>(result.Select(ToResponse).ToArray());
        })
        .WithName("GetProducts")
        .WithSummary("Get all products")
        .WithDescription("Returns all products, ordered by name.")
        .Produces<IReadOnlyList<ProductResponse>>(StatusCodes.Status200OK)
        .ProducesValidationProblem();

        products.MapGet("/{id:guid}", async Task<Results<Ok<ProductResponse>, NotFound>> (
            Guid id,
            IProductService productService,
            CancellationToken cancellationToken) =>
        {
            var product = await productService.GetByIdAsync(id, cancellationToken);
            return product is null
                ? TypedResults.NotFound()
                : TypedResults.Ok(ToResponse(product));
        })
        .WithName("GetProductById")
        .WithSummary("Get a product by ID")
        .WithDescription("Returns one product, or 404 if no product has the supplied ID.")
        .Produces<ProductResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        products.MapPost("", async Task<Created<ProductResponse>> (
            CreateProductRequest request,
            IProductService productService,
            ILoggerFactory loggerFactory,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateProductCommand(
                request.Name,
                request.Description,
                request.Price,
                request.Status,
                request.Quantity);
            var product = await productService.CreateAsync(command, cancellationToken);
            var response = ToResponse(product);

            var logger = loggerFactory.CreateLogger("CatalogueService.Api.Products");
            logger.LogInformation("Created product {ProductId}", response.Id);
            return TypedResults.Created($"/api/products/{response.Id}", response);
        })
        .WithName("CreateProduct")
        .WithSummary("Create a product")
        .WithDescription("Creates a product and returns its details.")
        .Produces<ProductResponse>(StatusCodes.Status201Created)
        .ProducesValidationProblem();

        if (app.Environment.IsDevelopment())
        {
            products.MapPost("/seed", async Task<Results<Ok<ProductSeedResponse>, ValidationProblem>> (
                IProductService productService,
                CancellationToken cancellationToken,
                int count = 5) =>
            {
                if (count is < 1 or > 1000)
                {
                    return TypedResults.ValidationProblem(new Dictionary<string, string[]>
                    {
                        ["count"] = ["Count must be between 1 and 1000."]
                    });
                }

                var runId = Guid.NewGuid().ToString("N");
                for (var index = 1; index <= count; index++)
                {
                    var seedProduct = SeedProducts[(index - 1) % SeedProducts.Length];
                    var command = new CreateProductCommand(
                        $"{seedProduct.Name} [Seed {runId}-{index:D4}]",
                        $"{seedProduct.Description} Seed run {runId}, item {index:D4}.",
                        seedProduct.Price,
                        CatalogueService.Domain.ProductStatus.Active,
                        seedProduct.Quantity);

                    await productService.CreateAsync(command, cancellationToken);
                }

                return TypedResults.Ok(new ProductSeedResponse(runId, count));
            })
            .WithName("SeedProducts")
            .WithSummary("Seed products for development")
            .WithDescription("Creates uniquely named sample products. Available only in the Development environment.")
            .Produces<ProductSeedResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem();
        }

        return app;
    }

    private static ProductResponse ToResponse(ProductDto product) =>
        new(product.Id, product.Name, product.Description, product.Price, product.Quantity, product.Status, product.CreatedAt);
}
