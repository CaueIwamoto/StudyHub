using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyHub.Api.Extensions;
using StudyHub.Application.DTOs;
using StudyHub.Application.Security;
using StudyHub.Application.Services;
using StudyHub.Domain.Enums;

namespace StudyHub.Api.Controllers;

/// <summary>
/// Gerenciamento de Propostas. O usuário autenticado é SEMPRE obtido das claims do JWT:
/// nenhum endpoint aceita PrestadorId/ClienteId vindos do corpo ou da query.
/// Não há endpoint para alterar status diretamente: as transições ocorrem apenas em aceitar/recusar.
/// </summary>
[ApiController]
[Authorize]
[Route("api")]
[Produces("application/json")]
public class PropostasController : ControllerBase
{
    private readonly IPropostaService _propostas;

    public PropostasController(IPropostaService propostas) => _propostas = propostas;

    /// <summary>Prestador envia proposta: aceite do valor inicial ou contraproposta. O pedido passa para EmNegociacao.</summary>
    [HttpPost("pedidos/{pedidoId:guid}/propostas")]
    [Authorize(Roles = Papeis.Prestador)]
    [ProducesResponseType(typeof(PropostaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PropostaResponse>> Criar(Guid pedidoId, [FromBody] CriarPropostaRequest request, CancellationToken cancellationToken)
    {
        var proposta = await _propostas.CriarAsync(User.ObterUsuarioId(), pedidoId, request, cancellationToken);
        return CreatedAtAction(nameof(Obter), new { id = proposta.Id }, proposta);
    }

    /// <summary>Cliente proprietário consulta as propostas recebidas no pedido (inclui perfil público do prestador).</summary>
    [HttpGet("pedidos/{pedidoId:guid}/propostas")]
    [Authorize(Roles = Papeis.Cliente)]
    [ProducesResponseType(typeof(IReadOnlyList<PropostaDetalhadaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<PropostaDetalhadaResponse>>> ListarPorPedido(
        Guid pedidoId, [FromQuery] StatusProposta? status, CancellationToken cancellationToken)
        => Ok(await _propostas.ListarPorPedidoAsync(User.ObterUsuarioId(), pedidoId, status, cancellationToken));

    /// <summary>Prestador consulta as próprias propostas e acompanha o status (filtro opcional por status).</summary>
    [HttpGet("propostas/minhas")]
    [Authorize(Roles = Papeis.Prestador)]
    [ProducesResponseType(typeof(IReadOnlyList<PropostaDetalhadaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<PropostaDetalhadaResponse>>> ListarMinhas(
        [FromQuery] StatusProposta? status, CancellationToken cancellationToken)
        => Ok(await _propostas.ListarMinhasAsync(User.ObterUsuarioId(), status, cancellationToken));

    /// <summary>Consulta uma proposta (somente o prestador autor ou o cliente proprietário do pedido).</summary>
    [HttpGet("propostas/{id:guid}")]
    [ProducesResponseType(typeof(PropostaDetalhadaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PropostaDetalhadaResponse>> Obter(Guid id, CancellationToken cancellationToken)
        => Ok(await _propostas.ObterAsync(User.ObterUsuarioId(), id, cancellationToken));

    /// <summary>
    /// Cliente proprietário aceita a proposta: proposta Aceita, pedido Fechado com ValorAcordado
    /// e demais propostas pendentes NaoSelecionada (tudo em uma transação).
    /// </summary>
    [HttpPatch("propostas/{id:guid}/aceitar")]
    [Authorize(Roles = Papeis.Cliente)]
    [ProducesResponseType(typeof(PropostaDetalhadaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PropostaDetalhadaResponse>> Aceitar(Guid id, CancellationToken cancellationToken)
        => Ok(await _propostas.AceitarAsync(User.ObterUsuarioId(), id, cancellationToken));

    /// <summary>Cliente proprietário recusa a proposta; sem outras pendentes, o pedido volta para Aberto.</summary>
    [HttpPatch("propostas/{id:guid}/recusar")]
    [Authorize(Roles = Papeis.Cliente)]
    [ProducesResponseType(typeof(PropostaDetalhadaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PropostaDetalhadaResponse>> Recusar(Guid id, CancellationToken cancellationToken)
        => Ok(await _propostas.RecusarAsync(User.ObterUsuarioId(), id, cancellationToken));
}
