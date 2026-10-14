using StudyHub.Application.DTOs;
using StudyHub.Application.Exceptions;
using StudyHub.Application.Mappings;
using StudyHub.Application.Validation;
using StudyHub.Domain.Interfaces;

namespace StudyHub.Application.Services;

public sealed class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _usuarios;
    private readonly IUnitOfWork _unitOfWork;

    public UsuarioService(IUsuarioRepository usuarios, IUnitOfWork unitOfWork)
    {
        _usuarios = usuarios;
        _unitOfWork = unitOfWork;
    }

    public async Task<UsuarioPerfilDto> ObterPerfilAsync(Guid usuarioId, CancellationToken cancellationToken = default)
    {
        var usuario = await _usuarios.GetByIdAsync(usuarioId, cancellationToken)
            ?? throw new NotFoundException("Usuário não encontrado.");
        return usuario.ToPerfilDto();
    }

    /// <summary>
    /// O id vem sempre do JWT (rota /me), logo somente o próprio usuário edita o próprio perfil.
    /// O DTO não possui Tipo/E-mail, portanto o papel não pode ser alterado.
    /// </summary>
    public async Task<UsuarioPerfilDto> AtualizarPerfilAsync(Guid usuarioId, AtualizarPerfilRequest request, CancellationToken cancellationToken = default)
    {
        RequestValidator.ValidateObject(request);

        var erros = new ValidationErrors();
        var areas = RequestValidator.NormalizarAreas(request.AreasConhecimento, nameof(request.AreasConhecimento), erros);
        erros.ThrowIfAny();

        var usuario = await _usuarios.GetByIdAsync(usuarioId, cancellationToken)
            ?? throw new NotFoundException("Usuário não encontrado.");

        var descricao = string.IsNullOrWhiteSpace(request.DescricaoPerfil) ? null : request.DescricaoPerfil.Trim();
        usuario.AtualizarPerfil(request.Nome.Trim(), descricao, areas);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return usuario.ToPerfilDto();
    }

    public async Task<UsuarioPublicoDto> ObterPublicoAsync(Guid usuarioId, CancellationToken cancellationToken = default)
    {
        var usuario = await _usuarios.GetByIdAsync(usuarioId, cancellationToken)
            ?? throw new NotFoundException("Usuário não encontrado.");
        return usuario.ToPublicoDto();
    }
}
