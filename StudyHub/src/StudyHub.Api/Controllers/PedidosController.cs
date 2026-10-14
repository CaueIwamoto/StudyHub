using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyHub.Api.Extensions;
using StudyHub.Application.DTOs;
using StudyHub.Application.Security;
using StudyHub.Application.Services;

namespace StudyHub.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/pedidos")]
[Produces("application/json")]
public class PedidosController : ControllerBase
{
    private readonly IPedidoService _pedidos;

    public PedidosController(IPedidoService pedidos) => _pedidos = pedidos;

    /// <summary>Cria um pedido (somente Cliente). O ClienteId vem do JWT.</summary>
    [HttpPost]
    [Authorize(Roles = Papeis.Cliente)]
    [ProducesResponseType(typeof(PedidoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PedidoDto>> Criar([FromBody] CriarPedidoRequest request, CancellationToken cancellationToken)
    {
        var pedido = await _pedidos.CriarAsync(User.ObterUsuarioId(), request, cancellationToken);
        return CreatedAtAction(nameof(Obter), new { id = pedido.Id }, pedido);
    }

    /// <summary>Lista pedidos com filtros (título, categoria, status, valor mínimo/máximo) e paginação.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<PedidoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<PedidoDto>>> Listar([FromQuery] PedidoFiltroRequest filtro, CancellationToken cancellationToken)
        => Ok(await _pedidos.ListarAsync(filtro, cancellationToken));

    /// <summary>Lista os pedidos disponíveis para propostas (Aberto ou EmNegociacao) — para o Prestador.</summary>
    [HttpGet("disponiveis")]
    [Authorize(Roles = Papeis.Prestador)]
    [ProducesResponseType(typeof(PagedResult<PedidoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResult<PedidoDto>>> ListarDisponiveis([FromQuery] PedidoFiltroRequest filtro, CancellationToken cancellationToken)
        => Ok(await _pedidos.ListarDisponiveisAsync(filtro, cancellationToken));

    /// <summary>Lista os pedidos do Cliente autenticado (mesmos filtros e paginação).</summary>
    [HttpGet("meus")]
    [Authorize(Roles = Papeis.Cliente)]
    [ProducesResponseType(typeof(PagedResult<PedidoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResult<PedidoDto>>> ListarMeus([FromQuery] PedidoFiltroRequest filtro, CancellationToken cancellationToken)
        => Ok(await _pedidos.ListarMeusAsync(User.ObterUsuarioId(), filtro, cancellationToken));

    /// <summary>Consulta um pedido pelo id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PedidoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PedidoDto>> Obter(Guid id, CancellationToken cancellationToken)
        => Ok(await _pedidos.ObterAsync(id, cancellationToken));

    /// <summary>Edita um pedido (somente o proprietário e somente enquanto estiver Aberto).</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = Papeis.Cliente)]
    [ProducesResponseType(typeof(PedidoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PedidoDto>> Atualizar(Guid id, [FromBody] AtualizarPedidoRequest request, CancellationToken cancellationToken)
        => Ok(await _pedidos.AtualizarAsync(User.ObterUsuarioId(), id, request, cancellationToken));

    /// <summary>Cancela um pedido (somente o proprietário; Fechados/Cancelados não podem ser cancelados). Propostas pendentes viram NaoSelecionada.</summary>
    [HttpPatch("{id:guid}/cancelar")]
    [Authorize(Roles = Papeis.Cliente)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancelar(Guid id, CancellationToken cancellationToken)
    {
        await _pedidos.CancelarAsync(User.ObterUsuarioId(), id, cancellationToken);
        return NoContent();
    }

    // Regra 10/11: NÃO existe endpoint público para mudar o status para EmNegociacao/Fechado.
    // Essas transições são feitas internamente pelo Gerenciamento de Propostas (PropostasController/PropostaService).
}
