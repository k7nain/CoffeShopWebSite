# CoffeeShop API

Production-ready ASP.NET Core 8 Web API using **Onion Architecture** for a coffee shop management system.

## Solution structure

```
CoffeShop/
├── CoffeeShop.sln
└── src/
    ├── CoffeeShop.Domain/
    ├── CoffeeShop.Application/
    ├── CoffeeShop.Infrastructure/
    └── CoffeeShop.API/
```

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [PostgreSQL](https://www.postgresql.org/) (optional in Development — InMemory DB is default)
- `dotnet-ef` global tool: `dotnet tool install --global dotnet-ef`

## Configuration

**Visual Studio (Development):** `appsettings.Development.json` sets `UseInMemoryDatabase: true` so the API runs without PostgreSQL. Swagger: `https://localhost:7007/swagger`.

For **PostgreSQL**, set `UseInMemoryDatabase: false` in `appsettings.Development.json` and update `ConnectionStrings:DefaultConnection` in `appsettings.json`.

`JwtSettings:SecretKey` must be at least **32 characters**.

## Run API

```powershell
dotnet run --project src/CoffeeShop.API/CoffeeShop.API.csproj
```

## Connect your own frontend

API base URL (Development): `https://localhost:7007`

CORS is enabled for `http://localhost:5173`, `http://localhost:3000`, and `https://localhost:5173`. Add your port in `Program.cs` if needed.

Send JWT as header: `Authorization: Bearer {accessToken}`

| Feature | Endpoints |
|---------|-----------|
| Menu / products | `GET /api/products`, `GET /api/products/category/{category}` |
| Basket / checkout | `POST /api/orders` (Customer role, body: `{ items: [{ productId, quantity }] }`) |
| Auth | `POST /api/auth/login`, `register`, `refresh` |
| Admin | `GET /api/admin/dashboard`, `orders`, `users`; `PUT /api/admin/orders/{id}/status` |

## Seed data

| Type | Details |
|------|---------|
| Admin | `admin@coffeeshop.com` / `Admin@123` |
| Products | 5 sample items |

## JWT lifetimes

- Access token: **15 minutes**
- Refresh token: **7 days**
