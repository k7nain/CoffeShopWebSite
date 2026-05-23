using Microsoft.Extensions.Configuration;
using Npgsql;

namespace CoffeeShop.Infrastructure.Configuration;

public static class DatabaseConnectionResolver
{
    public static string? Resolve(IConfiguration configuration)
    {
        var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL")
            ?? configuration["DATABASE_URL"];

        if (!string.IsNullOrWhiteSpace(databaseUrl))
        {
            return ConvertPostgresUrlToNpgsql(databaseUrl);
        }

        return configuration.GetConnectionString("DefaultConnection");
    }

    private static string ConvertPostgresUrlToNpgsql(string databaseUrl)
    {
        var uri = new Uri(databaseUrl);
        var userInfo = uri.UserInfo.Split(':', 2);
        var username = Uri.UnescapeDataString(userInfo[0]);
        var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty;
        var database = uri.AbsolutePath.TrimStart('/');

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.Port > 0 ? uri.Port : 5432,
            Database = database,
            Username = username,
            Password = password,
            SslMode = SslMode.Require,
            TrustServerCertificate = true
        };

        return builder.ConnectionString;
    }
}
