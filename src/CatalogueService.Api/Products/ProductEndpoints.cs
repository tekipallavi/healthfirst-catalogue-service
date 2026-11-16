using CatalogueService.Application.Products;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CatalogueService.Api.Products;

public static class ProductEndpoints
{
    public static WebApplication MapProducts(this WebApplication app)
    {
        var products = app.MapGroup("/api/products").WithTags("Products");

        products.MapGet("", async Task<Ok<IReadOnlyList<ProductResponse>>> (
            IProductService productService,
            CancellationToken cancellationToken) =>
        {
            var result = await productService.GetAllAsync(cancellationToken);
            return TypedResults.Ok<IReadOnlyList<ProductResponse>>(result.Select(ToResponse).ToArray());
        })
        .WithName("GetProducts")
        .WithSummary("Get all products")
        .WithDescription("Returns all products, ordered by name.")
        .Produces<IReadOnlyList<ProductResponse>>(StatusCodes.Status200OK);

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
                request.Status);
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

        return app;
    }

    private static ProductResponse ToResponse(ProductDto product) =>
        new(product.Id, product.Name, product.Description, product.Price, product.Status, product.CreatedAt);
}
