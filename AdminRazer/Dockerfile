# Use .NET 8 SDK to build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY ["AdminRazer/AdminRazer.csproj", "AdminRazer/"]
RUN dotnet restore "AdminRazer/AdminRazer.csproj"

# Copy everything else
COPY . .

WORKDIR /src/AdminRazer
RUN dotnet publish "AdminRazer.csproj" -c Release -o /app/publish --no-restore

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Listen on port 80
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

ENTRYPOINT ["dotnet", "AdminRazer.dll"]
