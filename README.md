# Firmeza
- This README is a lightweight guide. Add project-specific license and contribution guidelines as needed.
License & contribution

- Add CI/CD pipeline to build images and push to a registry.
- Add a `docker-compose.yml` to orchestrate the API with a database (Postgres/SQL Server).
Next steps / enhancements (optional)

- If migrations fail, verify the connection string and database accessibility.
- If the image build fails due to missing project references, make sure you run `docker build` from the repository root so the Docker context contains both `Web.Api` and `AdminRazer` folders.
Troubleshooting

- Example for connection string key: `ConnectionStrings__DefaultConnection` is commonly used for hierarchical configuration in ASP.NET Core.
- Connection strings and other secrets should be provided via environment variables at runtime (or via a secrets mechanism).
Notes about environment variables

Then visit http://localhost:8080 (or http://localhost:8080/swagger if Swagger is enabled).

```
  firmeza-webapi
  -e ConnectionStrings__DefaultConnection="<your-connection-string>" \
  -e ASPNETCORE_ENVIRONMENT=Production \
docker run --rm -p 8080:80 \
# run image mapping host port 8080 to container port 80

docker build -f Web.Api/Dockerfile -t firmeza-webapi .
# build image (from repo root)
```bash

This repository includes a `Web.Api/Dockerfile` and `.dockerignore`. Build the container from the repository root so the referenced projects are available during the image build:

4) Build & Run with Docker (Web.Api)

```
dotnet ef database update --project AdminRazer --startup-project Web.Api
dotnet tool install --global dotnet-ef  # if not installed
# from repository root
```bash

Migrations live in the `AdminRazer/Migrations` folder. To apply migrations locally (adjust the `--project` and `--startup-project` flags as your layout requires):

3) Database migrations

By default Kestrel will listen on the ports configured in the project (or the environment variable `ASPNETCORE_URLS`). When running with `dotnet run` the console output shows the actual URL(s). Swagger (if enabled) is usually available at `/swagger`.

```
dotnet run
```bash

From the `Web.Api` folder:

2) Run locally (API)

```
dotnet build
cd Web.Api
```bash

Open a terminal in the repository root and run:

1) Build locally (API)

Common tasks

- Optional: `dotnet-ef` tools for applying migrations locally: `dotnet tool install --global dotnet-ef`.
- Docker (if you want to build and run the API in a container): https://www.docker.com/
- .NET SDK 8.0 or later: https://dotnet.microsoft.com/
Prerequisites

- The solution includes EF Core migrations (see `Migrations/` inside `AdminRazer`).
- Target framework: .NET 8.0 (see `Web.Api/Web.Api.csproj`).
Quick overview

- Web.Api: ASP.NET Core Web API (API surface used by clients / SPA / mobile).
- AdminRazer: ASP.NET Core Razor pages / MVC application (Admin UI).
Projects

A multi-project ASP.NET Core solution consisting of an Admin Razor UI and a Web API back-end.


