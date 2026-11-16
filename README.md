# healthfirst-catalogue-service

Catalogue API for creating products and returning the product list.

## Azure SQL setup

The API reads its database connection string from `ConnectionStrings:Catalogue`.
The value in `src/CatalogueService.Api/appsettings.Development.json` is only a
placeholder. Replace it locally with a .NET user secret rather than committing
real credentials:

```sh
dotnet user-secrets init --project src/CatalogueService.Api
dotnet user-secrets set "ConnectionStrings:Catalogue" "<your Azure SQL connection string>" --project src/CatalogueService.Api
```

Install the EF Core command-line tool if needed, then create or update the
database schema using the checked-in migration:

```sh
dotnet tool install --global dotnet-ef --version 10.0.14
ASPNETCORE_ENVIRONMENT=Development dotnet ef database update \
  --project src/CatalogueService.Infrastructure \
  --startup-project src/CatalogueService.Api
```

The migration creates the `Products` table. Run the API with the Development
environment after setting the secret. The API does not create or update the
database automatically at startup.
