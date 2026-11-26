using System.Text.Json.Serialization;
using CatalogueService.Api.Products;
using CatalogueService.Application.Products;
using CatalogueService.Infrastructure;
using CatalogueService.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddValidation();
builder.Services.AddProblemDetails();
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddScoped<IProductService, ProductService>();
var connectionString = builder.Configuration.GetConnectionString("Catalogue")
    ?? throw new InvalidOperationException(
        "Connection string 'Catalogue' is missing. Configure it before starting the API.");
builder.Services.AddInfrastructure(connectionString);
builder.Services.AddHealthChecks().AddDbContextCheck<CatalogueDbContext>();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

//register routes
var api = app.MapGroup("/api");
api.MapProducts();
if (app.Environment.IsDevelopment())
{
    api.SeedProductDetails();
}

//register health check
app.MapHealthChecks("/health/dbcheck");

app.Run();
