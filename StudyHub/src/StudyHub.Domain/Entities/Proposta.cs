using StudyHub.Domain.Enums;
using StudyHub.Domain.Exceptions;

namespace StudyHub.Domain.Entities;

/// <summary>
/// Proposta (aceite do valor inicial ou contraproposta) enviada por um Prestador para um Pedido.
/// O histórico é preservado: propostas nunca são removidas, apenas mudam de status.
/// </summary>
public class Proposta
{
    // Construtor exigido pelo EF Core.
    private Proposta() { }

    public Guid Id { get; private set; }
    public Guid PedidoId { get; private set; }
    public Guid PrestadorId { get; private set; }
    public decimal ValorProposto { get; private set; }
    public string? Mensagem { get; private set; }
    public TipoProposta Tipo { get; private set; }
    public StatusProposta Status { get; private set; }
    public DateTime DataCriacao { get; private set; }

    public Pedido? Pedido { get; private set; }
    public Usuario? Prestador { get; private set; }

    public static Proposta Criar(Guid pedidoId, Guid prestadorId, decimal valorProposto, string? mensagem, TipoProposta tipo)
    {
        return new Proposta
        {
            Id = Guid.NewGuid(),
            PedidoId = pedidoId,
            PrestadorId = prestadorId,
            ValorProposto = valorProposto,
            Mensagem = mensagem,
            Tipo = tipo,
            Status = StatusProposta.Pendente,
            DataCriacao = DateTime.UtcNow
        };
    }

    public bool PertenceAoPrestador(Guid usuarioId) => PrestadorId == usuarioId;

    /// <summary>Regra 10: somente propostas Pendentes podem ser aceitas.</summary>
    public void Aceitar()
    {
        if (Status != StatusProposta.Pendente)
            throw new DomainException($"Propostas com status {Status} não podem ser aceitas. Apenas propostas Pendentes podem ser aceitas.");

        Status = StatusProposta.Aceita;
    }

    /// <summary>Regra 11: propostas Aceitas não podem ser recusadas (e somente Pendentes podem ser recusadas).</summary>
    public void Recusar()
    {
        if (Status != StatusProposta.Pendente)
            throw new DomainException($"Propostas com status {Status} não podem ser recusadas. Apenas propostas Pendentes podem ser recusadas.");

        Status = StatusProposta.Recusada;
    }

    /// <summary>Regra 14: usada quando outra proposta é aceita (ou o pedido é cancelado). Só afeta propostas Pendentes.</summary>
    public void MarcarComoNaoSelecionada()
    {
        if (Status != StatusProposta.Pendente)
            throw new DomainException($"Propostas com status {Status} não podem ser marcadas como não selecionadas.");

        Status = StatusProposta.NaoSelecionada;
    }
}
