using System.ComponentModel.DataAnnotations;
using StudyHub.Domain.Enums;

namespace StudyHub.Application.DTOs;

public class CriarPedidoRequest
{
    [Required(ErrorMessage = "O título é obrigatório.")]
    [StringLength(150, ErrorMessage = "O título deve ter no máximo 150 caracteres.")]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "A descrição é obrigatória.")]
    [StringLength(4000, ErrorMessage = "A descrição deve ter no máximo 4000 caracteres.")]
    public string Descricao { get; set; } = string.Empty;

    /// <summary>Opcional. Quando omitida, assume "Geral".</summary>
    [StringLength(80, ErrorMessage = "A categoria deve ter no máximo 80 caracteres.")]
    public string? Categoria { get; set; }

    /// <summary>Deve ser maior que zero (validado no serviço).</summary>
    public decimal ValorInicial { get; set; }
}

public class AtualizarPedidoRequest
{
    [Required(ErrorMessage = "O título é obrigatório.")]
    [StringLength(150, ErrorMessage = "O título deve ter no máximo 150 caracteres.")]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "A descrição é obrigatória.")]
    [StringLength(4000, ErrorMessage = "A descrição deve ter no máximo 4000 caracteres.")]
    public string Descricao { get; set; } = string.Empty;

    [StringLength(80, ErrorMessage = "A categoria deve ter no máximo 80 caracteres.")]
    public string? Categoria { get; set; }

    public decimal ValorInicial { get; set; }
}

/// <summary>Parâmetros de query da listagem de pedidos.</summary>
public class PedidoFiltroRequest
{
    public string? Titulo { get; set; }
    public string? Categoria { get; set; }
    public StatusPedido? Status { get; set; }
    public decimal? ValorMinimo { get; set; }
    public decimal? ValorMaximo { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "A página deve ser maior ou igual a 1.")]
    public int Pagina { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "O tamanho da página deve estar entre 1 e 100.")]
    public int TamanhoPagina { get; set; } = 20;
}

public sealed record PedidoDto(
    Guid Id,
    Guid ClienteId,
    string Titulo,
    string Descricao,
    string Categoria,
    decimal ValorInicial,
    decimal? ValorAcordado,
    StatusPedido Status,
    DateTime DataCriacao);

public sealed record PagedResult<T>(
    IReadOnlyList<T> Itens,
    int Pagina,
    int TamanhoPagina,
    int TotalItens,
    int TotalPaginas);
