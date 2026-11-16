using System.Text.Json.Serialization;
using CatalogueService.Api.Products;
using CatalogueService.Application.Products;
using CatalogueService.Infrastructure;

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

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapProducts();

app.Run();
