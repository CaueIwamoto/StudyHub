using StudyHub.Domain.Enums;

namespace StudyHub.Domain.Interfaces;

public class PedidoFiltro
{
    public Guid? ClienteId { get; init; }
    public string? Titulo { get; init; }
    public string? Categoria { get; init; }
    public StatusPedido? Status { get; init; }
    public decimal? ValorMinimo { get; init; }
    public decimal? ValorMaximo { get; init; }
    /// <summary>Quando true, retorna somente pedidos que ainda recebem propostas (Aberto ou EmNegociacao).</summary>
    public bool ApenasDisponiveis { get; init; }
    public int Pagina { get; init; } = 1;
    public int TamanhoPagina { get; init; } = 20;
}
