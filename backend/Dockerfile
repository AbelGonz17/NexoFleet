# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files for caching restore
COPY Directory.Build.props ./
COPY NuGet.Config ./
COPY backend/src/NexoFleet.Domain/NexoFleet.Domain.csproj backend/src/NexoFleet.Domain/
COPY backend/src/NexoFleet.Application/NexoFleet.Application.csproj backend/src/NexoFleet.Application/
COPY backend/src/NexoFleet.Infrastructure/NexoFleet.Infrastructure.csproj backend/src/NexoFleet.Infrastructure/
COPY backend/src/NexoFleet.Api/NexoFleet.Api.csproj backend/src/NexoFleet.Api/

RUN dotnet restore backend/src/NexoFleet.Api/NexoFleet.Api.csproj

# Copy the rest of the source code and build
COPY backend/src/ backend/src/
WORKDIR /src/backend/src/NexoFleet.Api
RUN dotnet publish -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "NexoFleet.Api.dll"]
