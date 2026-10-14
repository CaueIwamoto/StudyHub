using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StudyHub.Application.Abstractions;
using StudyHub.Domain.Interfaces;
using StudyHub.Infrastructure.Authentication;
using StudyHub.Infrastructure.Persistence;
using StudyHub.Infrastructure.Repositories;
using StudyHub.Infrastructure.Security;

namespace StudyHub.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString, JwtSettings jwtSettings)
    {
        services.AddDbContext<StudyHubDbContext>(options => options.UseNpgsql(connectionString));

        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IPedidoRepository, PedidoRepository>();
        services.AddScoped<IPropostaRepository, PropostaRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddSingleton<IPasswordHasher>(_ => new BCryptPasswordHasher());
        services.AddSingleton(jwtSettings);
        services.AddSingleton<ITokenService, JwtTokenService>();

        return services;
    }
}
