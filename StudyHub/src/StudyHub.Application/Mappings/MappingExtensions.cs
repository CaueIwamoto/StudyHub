using StudyHub.Application.DTOs;
using StudyHub.Domain.Entities;

namespace StudyHub.Application.Mappings;

public static class MappingExtensions
{
    public static UsuarioPerfilDto ToPerfilDto(this Usuario u) =>
        new(u.Id, u.Nome, u.Email, u.Tipo, u.DescricaoPerfil, u.AreasConhecimento.ToList(), u.DataCadastro);

    public static UsuarioPublicoDto ToPublicoDto(this Usuario u) =>
        new(u.Id, u.Nome, u.Tipo, u.DescricaoPerfil, u.AreasConhecimento.ToList(), u.DataCadastro);

    public static PedidoDto ToDto(this Pedido p) =>
        new(p.Id, p.ClienteId, p.Titulo, p.Descricao, p.Categoria, p.ValorInicial, p.ValorAcordado, p.Status, p.DataCriacao);

    public static PropostaResponse ToResponse(this Proposta p) =>
        new(p.Id, p.PedidoId, p.PrestadorId, p.ValorProposto, p.Mensagem, p.Tipo, p.Status, p.DataCriacao);

    /// <summary>Requer Pedido e Prestador carregados (Include).</summary>
    public static PropostaDetalhadaResponse ToDetalhadaResponse(this Proposta p)
    {
        if (p.Pedido is null || p.Prestador is null)
            throw new InvalidOperationException("Proposta carregada sem Pedido/Prestador.");

        return new PropostaDetalhadaResponse(
            p.Id, p.PedidoId, p.PrestadorId, p.ValorProposto, p.Mensagem, p.Tipo, p.Status, p.DataCriacao,
            p.Pedido.ToDto(),
            p.Prestador.ToPublicoDto());
    }
}
