# Крок 1 — збираємо API
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY CurrencyRates.API/CurrencyRates.API.csproj CurrencyRates.API/
COPY CurrencyRates.Application/CurrencyRates.Application.csproj CurrencyRates.Application/
COPY CurrencyRates.Domain/CurrencyRates.Domain.csproj CurrencyRates.Domain/
COPY CurrencyRates.Infrastructure/CurrencyRates.Infrastructure.csproj CurrencyRates.Infrastructure/

RUN dotnet restore CurrencyRates.API/CurrencyRates.API.csproj

COPY . .
RUN dotnet publish CurrencyRates.API/CurrencyRates.API.csproj -c Release -o /app/publish

# Крок 2 — фінальний образ без SDK
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "CurrencyRates.API.dll"]
