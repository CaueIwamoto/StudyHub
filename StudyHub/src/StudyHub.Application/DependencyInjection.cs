using Microsoft.Extensions.DependencyInjection;
using StudyHub.Application.Services;

namespace StudyHub.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUsuarioService, UsuarioService>();
        services.AddScoped<IPedidoService, PedidoService>();
        services.AddScoped<IPropostaService, PropostaService>();
        return services;
    }
}
