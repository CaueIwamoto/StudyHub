using System.Text;
using StudyHub.Application.Abstractions;
using StudyHub.Application.DTOs;
using StudyHub.Application.Exceptions;
using StudyHub.Application.Mappings;
using StudyHub.Application.Validation;
using StudyHub.Domain.Entities;
using StudyHub.Domain.Interfaces;

namespace StudyHub.Application.Services;

public sealed class AuthService : IAuthService
{
    private readonly IUsuarioRepository _usuarios;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _hasher;
    private readonly ITokenService _tokens;

    public AuthService(IUsuarioRepository usuarios, IUnitOfWork unitOfWork, IPasswordHasher hasher, ITokenService tokens)
    {
        _usuarios = usuarios;
        _unitOfWork = unitOfWork;
        _hasher = hasher;
        _tokens = tokens;
    }

    public async Task<UsuarioPerfilDto> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        RequestValidator.ValidateObject(request);

        var erros = new ValidationErrors();
        // BCrypt considera apenas os 72 primeiros bytes da senha.
        if (Encoding.UTF8.GetByteCount(request.Senha) > 72)
            erros.Add(nameof(request.Senha), "A senha deve ter no máximo 72 bytes.");
        var areas = RequestValidator.NormalizarAreas(request.AreasConhecimento, nameof(request.AreasConhecimento), erros);
        erros.ThrowIfAny();

        var email = NormalizarEmail(request.Email);
        if (await _usuarios.EmailExistsAsync(email, cancellationToken))
            throw new ConflictException("E-mail já cadastrado.");

        var descricao = string.IsNullOrWhiteSpace(request.DescricaoPerfil) ? null : request.DescricaoPerfil.Trim();
        var usuario = Usuario.Criar(
            request.Nome.Trim(),
            email,
            _hasher.Hash(request.Senha),
            request.Tipo,
            descricao,
            areas);

        await _usuarios.AddAsync(usuario, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return usuario.ToPerfilDto();
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        RequestValidator.ValidateObject(request);

        var usuario = await _usuarios.GetByEmailAsync(NormalizarEmail(request.Email), cancellationToken);

        // Mensagem única para e-mail inexistente e senha incorreta (evita enumeração de usuários).
        if (usuario is null || !_hasher.Verify(request.Senha, usuario.SenhaHash))
            throw new UnauthorizedException("E-mail ou senha inválidos.");

        var token = _tokens.GerarToken(usuario);
        return new AuthResponse(token.Token, token.ExpiraEmUtc, usuario.ToPerfilDto());
    }

    private static string NormalizarEmail(string email) => email.Trim().ToLowerInvariant();
}
