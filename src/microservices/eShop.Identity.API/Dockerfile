FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY eShop.Identity.API.sln .
COPY Identity.API/Identity.API.csproj Identity.API/Identity.API.csproj

RUN dotnet restore Identity.API/Identity.API.csproj

COPY Identity.API/ Identity.API/

WORKDIR /src/Identity.API

RUN dotnet publish Identity.API.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

WORKDIR /app

EXPOSE 5223
ENV ASPNETCORE_URLS=http://+:5223
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Identity.API.dll"]