FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY ["eShop.WebApp.sln", "./"]
COPY ["WebApp/WebApp.csproj", "WebApp/"]
COPY ["WebAppComponents/WebAppComponents.csproj", "WebAppComponents/"]

RUN dotnet restore "WebApp/WebApp.csproj"

COPY ["WebApp/", "WebApp/"]
COPY ["WebAppComponents/", "WebAppComponents/"]

WORKDIR /src/WebApp

RUN dotnet publish "WebApp.csproj" \
    -c Release \
    -o /app/publish 

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

WORKDIR /app

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "WebApp.dll"]

