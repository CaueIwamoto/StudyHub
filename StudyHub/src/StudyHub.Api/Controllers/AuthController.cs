using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyHub.Application.DTOs;
using StudyHub.Application.Services;

namespace StudyHub.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth) => _auth = auth;

    /// <summary>Cadastra um novo usuário (Cliente ou Prestador).</summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(UsuarioPerfilDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UsuarioPerfilDto>> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var usuario = await _auth.RegisterAsync(request, cancellationToken);
        return CreatedAtAction(nameof(UsuariosController.ObterPublico), "Usuarios", new { id = usuario.Id }, usuario);
    }

    /// <summary>Autentica e retorna o token JWT.</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var resposta = await _auth.LoginAsync(request, cancellationToken);
        return Ok(resposta);
    }
}
