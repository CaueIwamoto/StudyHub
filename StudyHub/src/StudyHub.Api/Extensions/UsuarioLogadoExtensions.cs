using System.Security.Claims;
using StudyHub.Application.Exceptions;
using StudyHub.Application.Security;

namespace StudyHub.Api.Extensions;

public static class UsuarioLogadoExtensions
{
    /// <summary>Obtém o id do usuário a partir do JWT (claim "sub"). Nunca do corpo/URL da requisição.</summary>
    public static Guid ObterUsuarioId(this ClaimsPrincipal principal)
    {
        var valor = principal.FindFirst(ClaimNames.UserId)?.Value
                    ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(valor, out var id))
            throw new UnauthorizedException("Token inválido.");

        return id;
    }
}
