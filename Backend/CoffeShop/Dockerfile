# CoffeeShop API — Railway / Docker
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY CoffeeShop.sln ./
COPY src/CoffeeShop.Domain/CoffeeShop.Domain.csproj src/CoffeeShop.Domain/
COPY src/CoffeeShop.Application/CoffeeShop.Application.csproj src/CoffeeShop.Application/
COPY src/CoffeeShop.Infrastructure/CoffeeShop.Infrastructure.csproj src/CoffeeShop.Infrastructure/
COPY src/CoffeeShop.API/CoffeeShop.API.csproj src/CoffeeShop.API/

RUN dotnet restore src/CoffeeShop.API/CoffeeShop.API.csproj

COPY src/ src/

RUN dotnet publish src/CoffeeShop.API/CoffeeShop.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production
ENV UseInMemoryDatabase=false

EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "CoffeeShop.API.dll"]
