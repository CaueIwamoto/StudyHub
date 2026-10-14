using Microsoft.EntityFrameworkCore;
using StudyHub.Application.DTOs;
using StudyHub.Application.Exceptions;
using StudyHub.Domain.Enums;
using StudyHub.Domain.Exceptions;

namespace StudyHub.Tests;

/// <summary>Testes do Gerenciamento de Propostas (serviços e repositórios reais sobre EF Core InMemory).</summary>
public class PropostaTests
{
    private static async Task<(UsuarioPerfilDto Cliente, UsuarioPerfilDto Prestador, PedidoDto Pedido)> PrepararAsync(
        TestEnvironment env, decimal valorInicial = 100m)
    {
        var cliente = await env.RegistrarAsync(TipoUsuario.Cliente, nome: "Ana Cliente");
        var prestador = await env.RegistrarAsync(TipoUsuario.Prestador, nome: "Paulo Professor");
        var pedido = await env.CriarPedidoAsync(cliente.Id, valor: valorInicial);
        return (cliente, prestador, pedido);
    }

    // ---------------------------------------------------------------- Cenários 1 a 4

    [Fact]
    public async Task Cenario1_prestador_aceita_valor_e_cliente_fecha()
    {
        using var env = new TestEnvironment();
        var (cliente, prestador, pedido) = await PrepararAsync(env, 100m);

        var proposta = await env.EnviarPropostaAsync(prestador.Id, pedido.Id, TipoProposta.AceiteValorInicial, 100m);

        Assert.Equal(StatusProposta.Pendente, proposta.Status);
        Assert.Equal(TipoProposta.AceiteValorInicial, proposta.Tipo);
        Assert.Equal(prestador.Id, proposta.PrestadorId);
        Assert.Equal(StatusPedido.EmNegociacao, (await env.Pedidos.ObterAsync(pedido.Id)).Status);

        // Cliente consulta a proposta e o perfil do prestador.
        var recebidas = await env.Propostas.ListarPorPedidoAsync(cliente.Id, pedido.Id, null);
        var recebida = Assert.Single(recebidas);
        Assert.Equal(100m, recebida.ValorProposto);
        Assert.Equal("Paulo Professor", recebida.Prestador.Nome);
        var perfil = await env.Usuarios.ObterPublicoAsync(prestador.Id);
        Assert.Equal(prestador.Id, perfil.Id);

        var aceita = await env.Propostas.AceitarAsync(cliente.Id, proposta.Id);

        Assert.Equal(StatusProposta.Aceita, aceita.Status);
        var final = await env.Pedidos.ObterAsync(pedido.Id);
        Assert.Equal(StatusPedido.Fechado, final.Status);
        Assert.Equal(100m, final.ValorAcordado);
        Assert.Equal(100m, final.ValorInicial);
    }

    [Fact]
    public async Task Cenario2_prestador_aceita_valor_e_cliente_recusa()
    {
        using var env = new TestEnvironment();
        var (cliente, prestador, pedido) = await PrepararAsync(env, 100m);

        var proposta = await env.EnviarPropostaAsync(prestador.Id, pedido.Id, TipoProposta.AceiteValorInicial, 100m);
        var consultada = await env.Propostas.ObterAsync(cliente.Id, proposta.Id);
        Assert.Equal(StatusProposta.Pendente, consultada.Status);

        var recusada = await env.Propostas.RecusarAsync(cliente.Id, proposta.Id);

        Assert.Equal(StatusProposta.Recusada, recusada.Status);
        var atual = await env.Pedidos.ObterAsync(pedido.Id);
        Assert.Equal(StatusPedido.Aberto, atual.Status);
        Assert.Null(atual.ValorAcordado);

        // Continua disponível para outros prestadores.
        var outro = await env.RegistrarAsync(TipoUsuario.Prestador);
        var nova = await env.EnviarPropostaAsync(outro.Id, pedido.Id, TipoProposta.AceiteValorInicial, 100m);
        Assert.Equal(StatusProposta.Pendente, nova.Status);
        Assert.Equal(StatusPedido.EmNegociacao, (await env.Pedidos.ObterAsync(pedido.Id)).Status);
    }

    [Fact]
    public async Task Cenario3_prestador_envia_contraproposta_e_cliente_aceita()
    {
        using var env = new TestEnvironment();
        var (cliente, prestador, pedido) = await PrepararAsync(env, 100m);

        var proposta = await env.EnviarPropostaAsync(prestador.Id, pedido.Id, TipoProposta.Contraproposta, 150m);

        Assert.Equal(StatusProposta.Pendente, proposta.Status);
        Assert.Equal(TipoProposta.Contraproposta, proposta.Tipo);
        Assert.Equal(150m, proposta.ValorProposto);
        Assert.Equal(StatusPedido.EmNegociacao, (await env.Pedidos.ObterAsync(pedido.Id)).Status);

        var detalhada = await env.Propostas.ObterAsync(cliente.Id, proposta.Id);
        Assert.Equal("Paulo Professor", detalhada.Prestador.Nome);

        var aceita = await env.Propostas.AceitarAsync(cliente.Id, proposta.Id);

        Assert.Equal(StatusProposta.Aceita, aceita.Status);
        var final = await env.Pedidos.ObterAsync(pedido.Id);
        Assert.Equal(StatusPedido.Fechado, final.Status);
        Assert.Equal(150m, final.ValorAcordado);
        Assert.Equal(100m, final.ValorInicial); // ValorInicial permanece
    }

    [Fact]
    public async Task Cenario4_prestador_envia_contraproposta_e_cliente_recusa()
    {
        using var env = new TestEnvironment();
        var (cliente, prestador, pedido) = await PrepararAsync(env, 100m);

        var proposta = await env.EnviarPropostaAsync(prestador.Id, pedido.Id, TipoProposta.Contraproposta, 150m);
        var recusada = await env.Propostas.RecusarAsync(cliente.Id, proposta.Id);

        Assert.Equal(StatusProposta.Recusada, recusada.Status);
        var atual = await env.Pedidos.ObterAsync(pedido.Id);
        Assert.Equal(StatusPedido.Aberto, atual.Status);
        Assert.Equal(100m, atual.ValorInicial);
        Assert.Null(atual.ValorAcordado);

        var disponiveis = await env.Pedidos.ListarDisponiveisAsync(new PedidoFiltroRequest());
        Assert.Contains(disponiveis.Itens, p => p.Id == pedido.Id);

        // O histórico é preservado.
        var historico = await env.Propostas.ListarMinhasAsync(prestador.Id, null);
        Assert.Equal(StatusProposta.Recusada, Assert.Single(historico).Status);
    }

    // ---------------------------------------------------------------- Regras de negócio

    [Fact]
    public async Task Prestador_envia_proposta_valida()
    {
        using var env = new TestEnvironment();
        var (_, prestador, pedido) = await PrepararAsync(env);

        var proposta = await env.EnviarPropostaAsync(prestador.Id, pedido.Id, TipoProposta.Contraproposta, 130.456m, "  Posso ajudar  ");

        Assert.NotEqual(Guid.Empty, proposta.Id);
        Assert.Equal(pedido.Id, proposta.PedidoId);
        Assert.Equal(prestador.Id, proposta.PrestadorId);
        Assert.Equal(130.46m, proposta.ValorProposto);
        Assert.Equal("Posso ajudar", proposta.Mensagem);
        Assert.Equal(StatusProposta.Pendente, proposta.Status);

        var salva = await env.Db.Propostas.SingleAsync();
        Assert.Equal(DateTimeKind.Utc, salva.DataCriacao.Kind);
    }

    [Fact]
    public async Task Cliente_nao_pode_enviar_proposta()
    {
        using var env = new TestEnvironment();
        var (cliente, _, pedido) = await PrepararAsync(env);
        var outroCliente = await env.RegistrarAsync(TipoUsuario.Cliente);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            env.EnviarPropostaAsync(outroCliente.Id, pedido.Id, TipoProposta.Contraproposta, 150m));
        await Assert.ThrowsAsync<ForbiddenException>(() =>
            env.EnviarPropostaAsync(cliente.Id, pedido.Id, TipoProposta.Contraproposta, 150m));
        Assert.Empty(await env.Db.Propostas.ToListAsync());
        Assert.Equal(StatusPedido.Aberto, (await env.Pedidos.ObterAsync(pedido.Id)).Status);
    }

    [Fact]
    public async Task Prestador_nao_pode_aceitar_a_propria_proposta()
    {
        using var env = new TestEnvironment();
        var (_, prestador, pedido) = await PrepararAsync(env);
        var proposta = await env.EnviarPropostaAsync(prestador.Id, pedido.Id, TipoProposta.Contraproposta, 150m);

        await Assert.ThrowsAsync<ForbiddenException>(() => env.Propostas.AceitarAsync(prestador.Id, proposta.Id));
        await Assert.ThrowsAsync<ForbiddenException>(() => env.Propostas.RecusarAsync(prestador.Id, proposta.Id));

        Assert.Equal(StatusProposta.Pendente, (await env.Propostas.ObterAsync(prestador.Id, proposta.Id)).Status);
        Assert.Equal(StatusPedido.EmNegociacao, (await env.Pedidos.ObterAsync(pedido.Id)).Status);
    }

    [Fact]
    public async Task Cliente_nao_pode_aceitar_nem_recusar_proposta_de_pedido_de_outro_cliente()
    {
        using var env = new TestEnvironment();
        var (_, prestador, pedido) = await PrepararAsync(env);
        var proposta = await env.EnviarPropostaAsync(prestador.Id, pedido.Id, TipoProposta.Contraproposta, 150m);
        var outroCliente = await env.RegistrarAsync(TipoUsuario.Cliente);
        await env.CriarPedidoAsync(outroCliente.Id); // tem pedido próprio, mas não é dono deste

        await Assert.ThrowsAsync<ForbiddenException>(() => env.Propostas.AceitarAsync(outroCliente.Id, proposta.Id));
        await Assert.ThrowsAsync<ForbiddenException>(() => env.Propostas.RecusarAsync(outroCliente.Id, proposta.Id));
        await Assert.ThrowsAsync<ForbiddenException>(() => env.Propostas.ObterAsync(outroCliente.Id, proposta.Id));
        await Assert.ThrowsAsync<ForbiddenException>(() => env.Propostas.ListarPorPedidoAsync(outroCliente.Id, pedido.Id, null));

        Assert.Equal(StatusProposta.Pendente, (await env.Db.Propostas.SingleAsync()).Status);
    }

    [Theory]
    [InlineData(TipoProposta.Contraproposta, 0.0)]
    [InlineData(TipoProposta.Contraproposta, -10.0)]
    [InlineData(TipoProposta.AceiteValorInicial, 0.0)]
    [InlineData(TipoProposta.AceiteValorInicial, 90.0)]   // aceite exige valor igual ao ValorInicial (100)
    [InlineData(TipoProposta.Contraproposta, 100.0)]      // contraproposta exige valor diferente do ValorInicial
    [InlineData(TipoProposta.Contraproposta, 0.004)]    // arredonda para 0,00
    public async Task Valor_proposto_invalido_e_rejeitado(TipoProposta tipo, double valor)
    {
        using var env = new TestEnvironment();
        var (_, prestador, pedido) = await PrepararAsync(env, 100m);

        var ex = await Assert.ThrowsAsync<RequestValidationException>(() =>
            env.EnviarPropostaAsync(prestador.Id, pedido.Id, tipo, (decimal)valor));

        Assert.Contains(nameof(CriarPropostaRequest.ValorProposto), ex.Errors.Keys);
        Assert.Empty(await env.Db.Propostas.ToListAsync());
        Assert.Equal(StatusPedido.Aberto, (await env.Pedidos.ObterAsync(pedido.Id)).Status);
    }

    [Fact]
    public async Task Tipo_ausente_ou_invalido_e_rejeitado()
    {
        using var env = new TestEnvironment();
        var (_, prestador, pedido) = await PrepararAsync(env);

        await Assert.ThrowsAsync<RequestValidationException>(() => env.Propostas.CriarAsync(
            prestador.Id, pedido.Id, new CriarPropostaRequest { Tipo = null, ValorProposto = 150m }));
        await Assert.ThrowsAsync<RequestValidationException>(() => env.Propostas.CriarAsync(
            prestador.Id, pedido.Id, new CriarPropostaRequest { Tipo = (TipoProposta)99, ValorProposto = 150m }));
    }

    [Fact]
    public async Task Proposta_duplicada_do_mesmo_prestador_no_mesmo_pedido_e_rejeitada()
    {
        using var env = new TestEnvironment();
        var (cliente, prestador, pedido) = await PrepararAsync(env);
        var primeira = await env.EnviarPropostaAsync(prestador.Id, pedido.Id, TipoProposta.Contraproposta, 150m);

        await Assert.ThrowsAsync<ConflictException>(() =>
            env.EnviarPropostaAsync(prestador.Id, pedido.Id, TipoProposta.Contraproposta, 160m));
        await Assert.ThrowsAsync<ConflictException>(() =>
            env.EnviarPropostaAsync(prestador.Id, pedido.Id, TipoProposta.AceiteValorInicial, 100m));
        Assert.Single(await env.Db.Propostas.ToListAsync());

        // Depois de recusada, o prestador pode enviar outra (o histórico mantém as duas).
        await env.Propostas.RecusarAsync(cliente.Id, primeira.Id);
        await env.EnviarPropostaAsync(prestador.Id, pedido.Id, TipoProposta.Contraproposta, 120m);
        Assert.Equal(2, await env.Db.Propostas.CountAsync());
    }

    [Fact]
    public async Task Pedido_pode_receber_propostas_de_varios_prestadores()
    {
        using var env = new TestEnvironment();
        var (cliente, prestador, pedido) = await PrepararAsync(env);
        var outro = await env.RegistrarAsync(TipoUsuario.Prestador);

        await env.EnviarPropostaAsync(prestador.Id, pedido.Id, TipoProposta.Contraproposta, 150m);
        await env.EnviarPropostaAsync(outro.Id, pedido.Id, TipoProposta.AceiteValorInicial, 100m);

        var recebidas = await env.Propostas.ListarPorPedidoAsync(cliente.Id, pedido.Id, null);
        Assert.Equal(2, recebidas.Count);
        Assert.Equal(StatusPedido.EmNegociacao, (await env.Pedidos.ObterAsync(pedido.Id)).Status);
    }

    [Fact]
    public async Task Proposta_para_pedido_fechado_ou_cancelado_e_rejeitada()
    {
        using var env = new TestEnvironment();
        var (cliente, prestador, pedidoFechado) = await PrepararAsync(env);
        var proposta = await env.EnviarPropostaAsync(prestador.Id, pedidoFechado.Id, TipoProposta.Contraproposta, 150m);
        await env.Propostas.AceitarAsync(cliente.Id, proposta.Id);

        var pedidoCancelado = await env.CriarPedidoAsync(cliente.Id);
        await env.Pedidos.CancelarAsync(cliente.Id, pedidoCancelado.Id);

        var outro = await env.RegistrarAsync(TipoUsuario.Prestador);
        await Assert.ThrowsAsync<DomainException>(() =>
            env.EnviarPropostaAsync(outro.Id, pedidoFechado.Id, TipoProposta.Contraproposta, 150m));
        await Assert.ThrowsAsync<DomainException>(() =>
            env.EnviarPropostaAsync(outro.Id, pedidoCancelado.Id, TipoProposta.Contraproposta, 150m));
        await Assert.ThrowsAsync<NotFoundException>(() =>
            env.EnviarPropostaAsync(outro.Id, Guid.NewGuid(), TipoProposta.Contraproposta, 150m));
    }

    [Fact]
    public async Task Nao_e_possivel_aceitar_proposta_recusada_ou_nao_selecionada_nem_recusar_proposta_aceita()
    {
        using var env = new TestEnvironment();
        var (cliente, prestadorA, pedido) = await PrepararAsync(env);
        var prestadorB = await env.RegistrarAsync(TipoUsuario.Prestador);
        var prestadorC = await env.RegistrarAsync(TipoUsuario.Prestador);

        var pA = await env.EnviarPropostaAsync(prestadorA.Id, pedido.Id, TipoProposta.Contraproposta, 150m);
        var pB = await env.EnviarPropostaAsync(prestadorB.Id, pedido.Id, TipoProposta.Contraproposta, 140m);
        var pC = await env.EnviarPropostaAsync(prestadorC.Id, pedido.Id, TipoProposta.Contraproposta, 130m);

        await env.Propostas.RecusarAsync(cliente.Id, pA.Id);
        await Assert.ThrowsAsync<DomainException>(() => env.Propostas.AceitarAsync(cliente.Id, pA.Id)); // Recusada
        await Assert.ThrowsAsync<DomainException>(() => env.Propostas.RecusarAsync(cliente.Id, pA.Id)); // já Recusada

        await env.Propostas.AceitarAsync(cliente.Id, pB.Id);
        await Assert.ThrowsAsync<DomainException>(() => env.Propostas.AceitarAsync(cliente.Id, pC.Id)); // NaoSelecionada
        await Assert.ThrowsAsync<DomainException>(() => env.Propostas.RecusarAsync(cliente.Id, pB.Id)); // Aceita
        await Assert.ThrowsAsync<DomainException>(() => env.Propostas.AceitarAsync(cliente.Id, pB.Id)); // já Aceita

        var pedidoFinal = await env.Pedidos.ObterAsync(pedido.Id);
        Assert.Equal(StatusPedido.Fechado, pedidoFinal.Status);
        Assert.Equal(140m, pedidoFinal.ValorAcordado);
    }

    [Fact]
    public async Task Aceite_atualiza_ValorAcordado_com_o_valor_da_proposta_aceita_e_preserva_ValorInicial()
    {
        using var env = new TestEnvironment();
        var (cliente, prestadorA, pedido) = await PrepararAsync(env, 100m);
        var prestadorB = await env.RegistrarAsync(TipoUsuario.Prestador);
        await env.EnviarPropostaAsync(prestadorA.Id, pedido.Id, TipoProposta.AceiteValorInicial, 100m);
        var pB = await env.EnviarPropostaAsync(prestadorB.Id, pedido.Id, TipoProposta.Contraproposta, 175.50m);

        Assert.Null((await env.Pedidos.ObterAsync(pedido.Id)).ValorAcordado);

        var aceita = await env.Propostas.AceitarAsync(cliente.Id, pB.Id);

        Assert.Equal(175.50m, aceita.Pedido.ValorAcordado);
        var final = await env.Pedidos.ObterAsync(pedido.Id);
        Assert.Equal(175.50m, final.ValorAcordado);
        Assert.Equal(100m, final.ValorInicial);
    }

    [Fact]
    public async Task Aceite_marca_as_outras_propostas_pendentes_como_NaoSelecionada_e_preserva_as_ja_recusadas()
    {
        using var env = new TestEnvironment();
        var (cliente, prestadorA, pedido) = await PrepararAsync(env);
        var prestadorB = await env.RegistrarAsync(TipoUsuario.Prestador);
        var prestadorC = await env.RegistrarAsync(TipoUsuario.Prestador);
        var prestadorD = await env.RegistrarAsync(TipoUsuario.Prestador);

        var pA = await env.EnviarPropostaAsync(prestadorA.Id, pedido.Id, TipoProposta.Contraproposta, 150m);
        var pB = await env.EnviarPropostaAsync(prestadorB.Id, pedido.Id, TipoProposta.AceiteValorInicial, 100m);
        var pC = await env.EnviarPropostaAsync(prestadorC.Id, pedido.Id, TipoProposta.Contraproposta, 120m);
        var pD = await env.EnviarPropostaAsync(prestadorD.Id, pedido.Id, TipoProposta.Contraproposta, 110m);
        await env.Propostas.RecusarAsync(cliente.Id, pD.Id);

        await env.Propostas.AceitarAsync(cliente.Id, pB.Id);

        var todas = (await env.Propostas.ListarPorPedidoAsync(cliente.Id, pedido.Id, null)).ToDictionary(p => p.Id);
        Assert.Equal(4, todas.Count); // histórico preservado
        Assert.Equal(StatusProposta.Aceita, todas[pB.Id].Status);
        Assert.Equal(StatusProposta.NaoSelecionada, todas[pA.Id].Status);
        Assert.Equal(StatusProposta.NaoSelecionada, todas[pC.Id].Status);
        Assert.Equal(StatusProposta.Recusada, todas[pD.Id].Status);
        Assert.Equal(1, todas.Values.Count(p => p.Status == StatusProposta.Aceita));
    }

    [Fact]
    public async Task Pedido_so_volta_para_Aberto_ao_recusar_a_ultima_proposta_pendente()
    {
        using var env = new TestEnvironment();
        var (cliente, prestadorA, pedido) = await PrepararAsync(env);
        var prestadorB = await env.RegistrarAsync(TipoUsuario.Prestador);
        var pA = await env.EnviarPropostaAsync(prestadorA.Id, pedido.Id, TipoProposta.Contraproposta, 150m);
        var pB = await env.EnviarPropostaAsync(prestadorB.Id, pedido.Id, TipoProposta.Contraproposta, 140m);

        await env.Propostas.RecusarAsync(cliente.Id, pA.Id);
        Assert.Equal(StatusPedido.EmNegociacao, (await env.Pedidos.ObterAsync(pedido.Id)).Status);

        await env.Propostas.RecusarAsync(cliente.Id, pB.Id);
        Assert.Equal(StatusPedido.Aberto, (await env.Pedidos.ObterAsync(pedido.Id)).Status);
    }

    [Fact]
    public async Task Pedido_em_negociacao_nao_pode_ser_editado_e_cancelamento_encerra_propostas_pendentes()
    {
        using var env = new TestEnvironment();
        var (cliente, prestador, pedido) = await PrepararAsync(env);
        var proposta = await env.EnviarPropostaAsync(prestador.Id, pedido.Id, TipoProposta.Contraproposta, 150m);

        await Assert.ThrowsAsync<DomainException>(() => env.Pedidos.AtualizarAsync(cliente.Id, pedido.Id,
            new AtualizarPedidoRequest { Titulo = "x", Descricao = "x", ValorInicial = 1m }));

        await env.Pedidos.CancelarAsync(cliente.Id, pedido.Id);

        Assert.Equal(StatusPedido.Cancelado, (await env.Pedidos.ObterAsync(pedido.Id)).Status);
        Assert.Equal(StatusProposta.NaoSelecionada, (await env.Propostas.ObterAsync(prestador.Id, proposta.Id)).Status);
    }

    [Fact]
    public async Task Consultas_de_propostas_respeitam_a_privacidade()
    {
        using var env = new TestEnvironment();
        var (cliente, prestador, pedido) = await PrepararAsync(env);
        var outroPrestador = await env.RegistrarAsync(TipoUsuario.Prestador);
        var proposta = await env.EnviarPropostaAsync(prestador.Id, pedido.Id, TipoProposta.Contraproposta, 150m);

        // Prestador autor e cliente dono veem; outro prestador não.
        Assert.Equal(proposta.Id, (await env.Propostas.ObterAsync(prestador.Id, proposta.Id)).Id);
        Assert.Equal(proposta.Id, (await env.Propostas.ObterAsync(cliente.Id, proposta.Id)).Id);
        await Assert.ThrowsAsync<ForbiddenException>(() => env.Propostas.ObterAsync(outroPrestador.Id, proposta.Id));
        await Assert.ThrowsAsync<ForbiddenException>(() => env.Propostas.ListarPorPedidoAsync(prestador.Id, pedido.Id, null));
        await Assert.ThrowsAsync<NotFoundException>(() => env.Propostas.ObterAsync(cliente.Id, Guid.NewGuid()));

        // "Minhas" mostra só as do próprio prestador; cliente não possui propostas enviadas.
        Assert.Single(await env.Propostas.ListarMinhasAsync(prestador.Id, null));
        Assert.Empty(await env.Propostas.ListarMinhasAsync(outroPrestador.Id, null));
        await Assert.ThrowsAsync<ForbiddenException>(() => env.Propostas.ListarMinhasAsync(cliente.Id, null));

        // Filtro por status.
        Assert.Single(await env.Propostas.ListarMinhasAsync(prestador.Id, StatusProposta.Pendente));
        Assert.Empty(await env.Propostas.ListarMinhasAsync(prestador.Id, StatusProposta.Aceita));
    }

    [Fact]
    public async Task Pedidos_disponiveis_incluem_Aberto_e_EmNegociacao_mas_nao_Fechado_nem_Cancelado()
    {
        using var env = new TestEnvironment();
        var cliente = await env.RegistrarAsync(TipoUsuario.Cliente);
        var prestador = await env.RegistrarAsync(TipoUsuario.Prestador);
        var aberto = await env.CriarPedidoAsync(cliente.Id, "Aberto");
        var negociacao = await env.CriarPedidoAsync(cliente.Id, "Negociacao");
        var fechado = await env.CriarPedidoAsync(cliente.Id, "Fechado");
        var cancelado = await env.CriarPedidoAsync(cliente.Id, "Cancelado");

        await env.EnviarPropostaAsync(prestador.Id, negociacao.Id, TipoProposta.Contraproposta, 150m);
        var pf = await env.EnviarPropostaAsync(prestador.Id, fechado.Id, TipoProposta.Contraproposta, 150m);
        await env.Propostas.AceitarAsync(cliente.Id, pf.Id);
        await env.Pedidos.CancelarAsync(cliente.Id, cancelado.Id);

        var resultado = await env.Pedidos.ListarDisponiveisAsync(new PedidoFiltroRequest());

        var ids = resultado.Itens.Select(p => p.Id).ToHashSet();
        Assert.Equal(new[] { aberto.Id, negociacao.Id }.ToHashSet(), ids);
    }

    [Fact]
    public async Task Segundo_aceite_em_outra_requisicao_nao_fecha_o_pedido_novamente()
    {
        // Duas "requisições" (DbContexts independentes) sobre o mesmo banco: o segundo aceite enxerga o pedido já Fechado.
        var options = TestEnvironment.NovasOpcoesInMemory();
        using var env1 = new TestEnvironment(options);
        using var env2 = new TestEnvironment(options);
        var (cliente, prestadorA, pedido) = await PrepararAsync(env1);
        var prestadorB = await env1.RegistrarAsync(TipoUsuario.Prestador);
        var pA = await env1.EnviarPropostaAsync(prestadorA.Id, pedido.Id, TipoProposta.Contraproposta, 150m);
        var pB = await env1.EnviarPropostaAsync(prestadorB.Id, pedido.Id, TipoProposta.Contraproposta, 120m);

        await env1.Propostas.AceitarAsync(cliente.Id, pA.Id);
        await Assert.ThrowsAsync<DomainException>(() => env2.Propostas.AceitarAsync(cliente.Id, pB.Id));

        var propostas = await env2.Db.Propostas.AsNoTracking().ToListAsync();
        Assert.Single(propostas, p => p.Status == StatusProposta.Aceita);
        Assert.Equal(150m, (await env2.Pedidos.ObterAsync(pedido.Id)).ValorAcordado);
    }
}
