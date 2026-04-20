FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY eShop.PaymentProcessor.sln .
COPY PaymentProcessor/PaymentProcessor.csproj PaymentProcessor/PaymentProcessor.csproj

RUN dotnet restore PaymentProcessor/PaymentProcessor.csproj

COPY PaymentProcessor/ PaymentProcessor/

WORKDIR /src/PaymentProcessor

RUN dotnet publish PaymentProcessor.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

WORKDIR /app

EXPOSE 5226

ENV ASPNETCORE_URLS=http://+:5226
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "PaymentProcessor.dll"]
