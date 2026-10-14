using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyHub.Api.Extensions;
using StudyHub.Application.DTOs;
using StudyHub.Application.Services;

namespace StudyHub.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/usuarios")]
[Produces("application/json")]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioService _usuarios;

    public UsuariosController(IUsuarioService usuarios) => _usuarios = usuarios;

    /// <summary>Consulta o próprio perfil (requer JWT).</summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UsuarioPerfilDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UsuarioPerfilDto>> ObterMeuPerfil(CancellationToken cancellationToken)
        => Ok(await _usuarios.ObterPerfilAsync(User.ObterUsuarioId(), cancellationToken));

    /// <summary>Atualiza o próprio perfil. Nome, descrição e áreas de conhecimento; o papel não é alterável.</summary>
    [HttpPut("me")]
    [ProducesResponseType(typeof(UsuarioPerfilDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UsuarioPerfilDto>> AtualizarMeuPerfil([FromBody] AtualizarPerfilRequest request, CancellationToken cancellationToken)
        => Ok(await _usuarios.AtualizarPerfilAsync(User.ObterUsuarioId(), request, cancellationToken));

    /// <summary>Consulta o perfil público de um usuário (sem e-mail ou dados sensíveis). Não exige JWT.</summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(UsuarioPublicoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UsuarioPublicoDto>> ObterPublico(Guid id, CancellationToken cancellationToken)
        => Ok(await _usuarios.ObterPublicoAsync(id, cancellationToken));
}
