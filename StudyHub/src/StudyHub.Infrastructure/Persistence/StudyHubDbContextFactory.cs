using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using StudyHub.Infrastructure.Configuration;

namespace StudyHub.Infrastructure.Persistence;

/// <summary>Usada apenas pelas ferramentas do EF (dotnet ef migrations / database update).</summary>
public class StudyHubDbContextFactory : IDesignTimeDbContextFactory<StudyHubDbContext>
{
    public StudyHubDbContext CreateDbContext(string[] args)
    {
        DotEnvLoader.Load();

        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? PostgresConnectionString.FromEnvironment(requirePassword: false);

        var options = new DbContextOptionsBuilder<StudyHubDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new StudyHubDbContext(options);
    }
}
