using System.ComponentModel.DataAnnotations;
using StudyHub.Domain.Enums;

namespace StudyHub.Application.DTOs;

/// <summary>
/// Corpo para enviar uma proposta. O PrestadorId NÃO é informado aqui: vem do JWT.
/// Tipo AceiteValorInicial exige valor igual ao ValorInicial do pedido; Contraproposta exige valor diferente.
/// </summary>
public class CriarPropostaRequest
{
    [Required(ErrorMessage = "O tipo da proposta é obrigatório (AceiteValorInicial ou Contraproposta).")]
    public TipoProposta? Tipo { get; set; }

    /// <summary>Deve ser maior que zero (validado no serviço).</summary>
    public decimal ValorProposto { get; set; }

    [StringLength(1000, ErrorMessage = "A mensagem deve ter no máximo 1000 caracteres.")]
    public string? Mensagem { get; set; }
}

public sealed record PropostaResponse(
    Guid Id,
    Guid PedidoId,
    Guid PrestadorId,
    decimal ValorProposto,
    string? Mensagem,
    TipoProposta Tipo,
    StatusProposta Status,
    DateTime DataCriacao);

/// <summary>
/// Proposta com o resumo do pedido (inclui status e ValorAcordado, ou seja, o resultado da negociação)
/// e o perfil público do prestador (sem e-mail).
/// </summary>
public sealed record PropostaDetalhadaResponse(
    Guid Id,
    Guid PedidoId,
    Guid PrestadorId,
    decimal ValorProposto,
    string? Mensagem,
    TipoProposta Tipo,
    StatusProposta Status,
    DateTime DataCriacao,
    PedidoDto Pedido,
    UsuarioPublicoDto Prestador);
