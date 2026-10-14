using Npgsql;

namespace StudyHub.Infrastructure.Configuration;

public static class PostgresConnectionString
{
    /// <summary>Monta a connection string a partir das variáveis POSTGRES_*.</summary>
    public static string FromEnvironment(bool requirePassword = true)
    {
        var password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");
        if (requirePassword && string.IsNullOrEmpty(password))
            throw new InvalidOperationException(
                "Connection string não configurada. Defina POSTGRES_PASSWORD (e demais variáveis POSTGRES_*) no arquivo .env " +
                "ou em variáveis de ambiente, ou defina ConnectionStrings__DefaultConnection.");

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "localhost",
            Port = int.TryParse(Environment.GetEnvironmentVariable("POSTGRES_PORT"), out var port) ? port : 5432,
            Database = Environment.GetEnvironmentVariable("POSTGRES_DB") ?? "studyhub",
            Username = Environment.GetEnvironmentVariable("POSTGRES_USER") ?? "studyhub"
        };
        if (!string.IsNullOrEmpty(password))
            builder.Password = password;

        return builder.ConnectionString;
    }
}
