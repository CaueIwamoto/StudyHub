using StudyHub.Application.DTOs;

namespace StudyHub.Application.Services;

public interface IAuthService
{
    Task<UsuarioPerfilDto> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}
