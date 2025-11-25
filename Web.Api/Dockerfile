# Use .NET 8 SDK to build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj(s) and restore as distinct layers
COPY ["Web.Api/Web.Api.csproj", "Web.Api/"]
COPY ["AdminRazer/AdminRazer.csproj", "AdminRazer/"]
RUN dotnet restore "Web.Api/Web.Api.csproj"

# Copy everything required for build
COPY . .

# Remove appsettings files from referenced project to prevent duplicate publish outputs
# (When referenced projects include appsettings.json as content it can cause NETSDK1152)
RUN rm -f /src/AdminRazer/appsettings.json /src/AdminRazer/appsettings.Development.json || true

WORKDIR /src/Web.Api
RUN dotnet publish "Web.Api.csproj" -c Release -o /app/publish

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Listen on port 80 inside container
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

ENTRYPOINT ["dotnet", "Web.Api.dll"]
