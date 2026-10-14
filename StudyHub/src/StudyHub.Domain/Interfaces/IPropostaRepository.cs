using StudyHub.Domain.Entities;
using StudyHub.Domain.Enums;

namespace StudyHub.Domain.Interfaces;

public interface IPropostaRepository
{
    /// <summary>Retorna a proposta rastreada (tracking), com Pedido e Prestador carregados.</summary>
    Task<Proposta?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Obtém apenas o PedidoId da proposta, sem rastrear entidades (usado antes de bloquear o pedido).</summary>
    Task<Guid?> ObterPedidoIdAsync(Guid propostaId, CancellationToken cancellationToken = default);

    Task AddAsync(Proposta proposta, CancellationToken cancellationToken = default);

    /// <summary>Existe proposta Pendente deste prestador neste pedido? (regra 5)</summary>
    Task<bool> ExistePendenteDoPrestadorAsync(Guid pedidoId, Guid prestadorId, CancellationToken cancellationToken = default);

    /// <summary>Existe alguma proposta Pendente no pedido, exceto a informada? (regra 15)</summary>
    Task<bool> ExisteOutraPendenteAsync(Guid pedidoId, Guid exceto, CancellationToken cancellationToken = default);

    /// <summary>Propostas Pendentes do pedido, rastreadas, exceto a informada (para marcá-las como NaoSelecionada).</summary>
    Task<IReadOnlyList<Proposta>> ListarPendentesDoPedidoAsync(Guid pedidoId, Guid? exceto, CancellationToken cancellationToken = default);

    /// <summary>Todas as propostas do pedido (histórico), somente leitura, com Pedido e Prestador.</summary>
    Task<IReadOnlyList<Proposta>> ListarPorPedidoAsync(Guid pedidoId, StatusProposta? status, CancellationToken cancellationToken = default);

    /// <summary>Todas as propostas do prestador (histórico), somente leitura, com Pedido e Prestador.</summary>
    Task<IReadOnlyList<Proposta>> ListarPorPrestadorAsync(Guid prestadorId, StatusProposta? status, CancellationToken cancellationToken = default);
}
