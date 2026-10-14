using StudyHub.Domain.Enums;
using StudyHub.Domain.Exceptions;

namespace StudyHub.Domain.Entities;

public class Pedido
{
    // Construtor exigido pelo EF Core.
    private Pedido()
    {
        Titulo = string.Empty;
        Descricao = string.Empty;
        Categoria = string.Empty;
    }

    public Guid Id { get; private set; }
    public Guid ClienteId { get; private set; }
    public string Titulo { get; private set; }
    public string Descricao { get; private set; }
    public string Categoria { get; private set; }
    public decimal ValorInicial { get; private set; }
    public decimal? ValorAcordado { get; private set; }
    public StatusPedido Status { get; private set; }
    public DateTime DataCriacao { get; private set; }

    public Usuario? Cliente { get; private set; }
    public ICollection<Proposta> Propostas { get; private set; } = new List<Proposta>();

    public static Pedido Criar(Guid clienteId, string titulo, string descricao, string categoria, decimal valorInicial)
    {
        return new Pedido
        {
            Id = Guid.NewGuid(),
            ClienteId = clienteId,
            Titulo = titulo,
            Descricao = descricao,
            Categoria = categoria,
            ValorInicial = valorInicial,
            ValorAcordado = null,
            Status = StatusPedido.Aberto,
            DataCriacao = DateTime.UtcNow
        };
    }

    public bool PertenceA(Guid usuarioId) => ClienteId == usuarioId;

    /// <summary>Regras 8 e 9: somente pedidos Abertos podem ser editados.</summary>
    public void Atualizar(string titulo, string descricao, string categoria, decimal valorInicial)
    {
        if (Status != StatusPedido.Aberto)
            throw new DomainException($"Pedidos com status {Status} não podem ser editados. Apenas pedidos Abertos podem ser editados.");

        Titulo = titulo;
        Descricao = descricao;
        Categoria = categoria;
        ValorInicial = valorInicial;
    }

    /// <summary>Pedidos Fechados ou já Cancelados não podem ser cancelados.</summary>
    public void Cancelar()
    {
        if (Status is StatusPedido.Fechado or StatusPedido.Cancelado)
            throw new DomainException($"Pedidos com status {Status} não podem ser cancelados.");

        Status = StatusPedido.Cancelado;
    }

    /// <summary>
    /// Chamado quando o pedido recebe uma proposta. Aberto -> EmNegociacao; se já estiver EmNegociacao não muda.
    /// Pedidos Fechados ou Cancelados não aceitam propostas (regra 9).
    /// </summary>
    public void RegistrarProposta()
    {
        if (Status == StatusPedido.Aberto)
        {
            Status = StatusPedido.EmNegociacao;
            return;
        }

        if (Status != StatusPedido.EmNegociacao)
            throw new DomainException($"Pedidos com status {Status} não aceitam novas propostas.");
    }

    /// <summary>Regras 12 e 13: EmNegociacao -> Fechado, registrando o ValorAcordado. O ValorInicial é preservado.</summary>
    public void Fechar(decimal valorAcordado)
    {
        if (Status != StatusPedido.EmNegociacao)
            throw new DomainException($"Pedidos com status {Status} não podem ser fechados. Apenas pedidos Em Negociação podem ser fechados.");
        if (valorAcordado <= 0)
            throw new DomainException("O valor acordado deve ser maior que zero.");

        Status = StatusPedido.Fechado;
        ValorAcordado = valorAcordado;
    }

    /// <summary>Regra 15: EmNegociacao -> Aberto quando não restam propostas pendentes.</summary>
    public void Reabrir()
    {
        if (Status != StatusPedido.EmNegociacao)
            throw new DomainException($"Pedidos com status {Status} não podem ser reabertos.");

        Status = StatusPedido.Aberto;
    }

    /// <summary>Pedido ainda recebe propostas (Aberto ou EmNegociacao).</summary>
    public bool EstaDisponivel => Status is StatusPedido.Aberto or StatusPedido.EmNegociacao;
}
