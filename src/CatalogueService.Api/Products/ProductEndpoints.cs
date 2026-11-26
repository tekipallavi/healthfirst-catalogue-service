using CatalogueService.Application.Products;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CatalogueService.Api.Products;

public static class ProductEndpoints
{
    private static readonly string[] SortFields = [ "name", "price", "status", "createdAt" ]; 
    private static readonly string[] SortOrders = [ "asc", "desc" ];
    public static RouteGroupBuilder MapProducts(this RouteGroupBuilder api)
    {
        var products = api.MapGroup("/products").WithTags("Products");

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

        
        return api;
    }

    private static ProductResponse ToResponse(ProductDto product) =>
        new(product.Id, product.Name, product.Description, product.Price, product.Quantity, product.Status, product.CreatedAt);
}
