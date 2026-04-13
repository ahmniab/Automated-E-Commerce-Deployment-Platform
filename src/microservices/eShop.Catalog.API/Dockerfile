FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY eShop.Catalog.API.sln .
COPY Catalog.API/Catalog.API.csproj Catalog.API/Catalog.API.csproj
RUN dotnet restore Catalog.API/Catalog.API.csproj
COPY Catalog.API/ Catalog.API/
WORKDIR /src/Catalog.API
RUN dotnet publish Catalog.API.csproj -c Release -o /app/publish
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Catalog.API.dll"]
