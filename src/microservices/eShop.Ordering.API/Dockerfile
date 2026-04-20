# ============================================================
# Stage 1: BUILD
# Ordering API has 3 projects that must all be included:
#   - Ordering.API          → web layer (endpoints, controllers)
#   - Ordering.Domain       → business logic (DDD entities)
#   - Ordering.Infrastructure → database layer (EF Core, migrations)
# ============================================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# Copy solution and all .csproj files before restoring
COPY eShop.Ordering.API.sln .
COPY Ordering.API/Ordering.API.csproj           Ordering.API/Ordering.API.csproj
COPY Ordering.Domain/Ordering.Domain.csproj     Ordering.Domain/Ordering.Domain.csproj
COPY Ordering.Infrastructure/Ordering.Infrastructure.csproj  Ordering.Infrastructure/Ordering.Infrastructure.csproj

# Restore fetches packages for all 3 projects at once
# because Ordering.API.csproj has ProjectReferences to the other two
RUN dotnet restore Ordering.API/Ordering.API.csproj

# Copy source code for all projects
COPY Ordering.API/           Ordering.API/
COPY Ordering.Domain/        Ordering.Domain/
COPY Ordering.Infrastructure/ Ordering.Infrastructure/
COPY Shared/                 Shared/

WORKDIR /src/Ordering.API

# Publish automatically includes referenced projects (Domain + Infrastructure)
RUN dotnet publish Ordering.API.csproj -c Release -o /app/publish

# ============================================================
# Stage 2: RUNTIME
# ============================================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

WORKDIR /app

EXPOSE 5224

ENV ASPNETCORE_URLS=http://+:5224
ENV ASPNETCORE_ENVIRONMENT=Production

# PostgreSQL connection string — set in docker-compose
ENV ConnectionStrings__OrderingDb=""

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Ordering.API.dll"]
