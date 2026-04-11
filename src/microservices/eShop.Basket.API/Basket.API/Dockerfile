FROM mcr.microsoft.com/dotnet/sdk:10.0 AS builder

WORKDIR /src

COPY Basket.API.csproj .

RUN dotnet restore

COPY . .

RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime   
 
WORKDIR /app

COPY --from=builder /app .

ENTRYPOINT ["dotnet", "Basket.API.dll"]