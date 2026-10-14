using StudyHub.Application.DTOs;

namespace StudyHub.Application.Services;

public interface IUsuarioService
{
    Task<UsuarioPerfilDto> ObterPerfilAsync(Guid usuarioId, CancellationToken cancellationToken = default);
    Task<UsuarioPerfilDto> AtualizarPerfilAsync(Guid usuarioId, AtualizarPerfilRequest request, CancellationToken cancellationToken = default);
    Task<UsuarioPublicoDto> ObterPublicoAsync(Guid usuarioId, CancellationToken cancellationToken = default);
}
