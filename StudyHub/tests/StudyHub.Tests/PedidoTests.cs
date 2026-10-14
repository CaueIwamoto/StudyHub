using Microsoft.EntityFrameworkCore;
using StudyHub.Application.DTOs;
using StudyHub.Application.Exceptions;
using StudyHub.Domain.Enums;

namespace StudyHub.Tests;

public class PedidoTests
{
    [Fact]
    public async Task Cliente_cria_pedido_aberto_sem_valor_acordado_e_com_ClienteId_do_usuario()
    {
        using var env = new TestEnvironment();
        var cliente = await env.RegistrarAsync(TipoUsuario.Cliente);

        var pedido = await env.CriarPedidoAsync(cliente.Id, valor: 100m);

        Assert.NotEqual(Guid.Empty, pedido.Id);
        Assert.Equal(cliente.Id, pedido.ClienteId);
        Assert.Equal(StatusPedido.Aberto, pedido.Status);
        Assert.Null(pedido.ValorAcordado);
        Assert.Equal(100m, pedido.ValorInicial);
        Assert.Equal("Redação", pedido.Categoria);

        var salvo = await env.Db.Pedidos.SingleAsync();
        Assert.Equal(DateTimeKind.Utc, salvo.DataCriacao.Kind);
    }

    [Fact]
    public async Task Pedido_sem_categoria_assume_categoria_padrao()
    {
        using var env = new TestEnvironment();
        var cliente = await env.RegistrarAsync(TipoUsuario.Cliente);

        var pedido = await env.CriarPedidoAsync(cliente.Id, categoria: null);

        Assert.Equal("Geral", pedido.Categoria);
    }

    [Fact]
    public async Task Prestador_nao_pode_criar_pedido()
    {
        using var env = new TestEnvironment();
        var prestador = await env.RegistrarAsync(TipoUsuario.Prestador);

        await Assert.ThrowsAsync<ForbiddenException>(() => env.CriarPedidoAsync(prestador.Id));

        Assert.Equal(0, await env.Db.Pedidos.CountAsync());
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(-10.0)]
    [InlineData(0.004)] // arredonda para 0,00
    public async Task Valor_inicial_menor_ou_igual_a_zero_e_rejeitado(double valor)
    {
        using var env = new TestEnvironment();
        var cliente = await env.RegistrarAsync(TipoUsuario.Cliente);

        var ex = await Assert.ThrowsAsync<RequestValidationException>(() =>
            env.CriarPedidoAsync(cliente.Id, valor: (decimal)valor));

        Assert.Contains(nameof(CriarPedidoRequest.ValorInicial), ex.Errors.Keys);
        Assert.Equal(0, await env.Db.Pedidos.CountAsync());
    }

    [Fact]
    public async Task Titulo_e_descricao_sao_obrigatorios()
    {
        using var env = new TestEnvironment();
        var cliente = await env.RegistrarAsync(TipoUsuario.Cliente);

        var ex = await Assert.ThrowsAsync<RequestValidationException>(() =>
            env.Pedidos.CriarAsync(cliente.Id, new CriarPedidoRequest { Titulo = " ", Descricao = "", ValorInicial = 10m }));

        Assert.Contains(nameof(CriarPedidoRequest.Titulo), ex.Errors.Keys);
        Assert.Contains(nameof(CriarPedidoRequest.Descricao), ex.Errors.Keys);
    }

    [Fact]
    public async Task Cliente_edita_o_proprio_pedido_aberto()
    {
        using var env = new TestEnvironment();
        var cliente = await env.RegistrarAsync(TipoUsuario.Cliente);
        var pedido = await env.CriarPedidoAsync(cliente.Id);

        var atualizado = await env.Pedidos.AtualizarAsync(cliente.Id, pedido.Id, new AtualizarPedidoRequest
        {
            Titulo = "Novo título",
            Descricao = "Nova descrição",
            Categoria = "Matemática",
            ValorInicial = 250.50m
        });

        Assert.Equal("Novo título", atualizado.Titulo);
        Assert.Equal("Nova descrição", atualizado.Descricao);
        Assert.Equal("Matemática", atualizado.Categoria);
        Assert.Equal(250.50m, atualizado.ValorInicial);
        Assert.Equal(StatusPedido.Aberto, atualizado.Status);
        Assert.Equal(cliente.Id, atualizado.ClienteId);

        var salvo = await env.Db.Pedidos.AsNoTracking().SingleAsync();
        Assert.Equal("Novo título", salvo.Titulo);
    }

    [Fact]
    public async Task Usuario_nao_pode_editar_pedido_de_outro_cliente()
    {
        using var env = new TestEnvironment();
        var dono = await env.RegistrarAsync(TipoUsuario.Cliente);
        var outro = await env.RegistrarAsync(TipoUsuario.Cliente);
        var pedido = await env.CriarPedidoAsync(dono.Id, titulo: "Título original");

        await Assert.ThrowsAsync<ForbiddenException>(() => env.Pedidos.AtualizarAsync(outro.Id, pedido.Id,
            new AtualizarPedidoRequest { Titulo = "Invadido", Descricao = "x", ValorInicial = 1m }));

        var salvo = await env.Db.Pedidos.AsNoTracking().SingleAsync();
        Assert.Equal("Título original", salvo.Titulo);
    }

    [Fact]
    public async Task Editar_pedido_inexistente_lanca_nao_encontrado()
    {
        using var env = new TestEnvironment();
        var cliente = await env.RegistrarAsync(TipoUsuario.Cliente);

        await Assert.ThrowsAsync<NotFoundException>(() => env.Pedidos.AtualizarAsync(cliente.Id, Guid.NewGuid(),
            new AtualizarPedidoRequest { Titulo = "x", Descricao = "x", ValorInicial = 1m }));
    }

    [Fact]
    public async Task Proprietario_cancela_pedido_e_depois_nao_pode_edita_lo_nem_cancelar_novamente()
    {
        using var env = new TestEnvironment();
        var cliente = await env.RegistrarAsync(TipoUsuario.Cliente);
        var pedido = await env.CriarPedidoAsync(cliente.Id);

        await env.Pedidos.CancelarAsync(cliente.Id, pedido.Id);

        var cancelado = await env.Pedidos.ObterAsync(pedido.Id);
        Assert.Equal(StatusPedido.Cancelado, cancelado.Status);

        await Assert.ThrowsAsync<Domain.Exceptions.DomainException>(() => env.Pedidos.AtualizarAsync(cliente.Id, pedido.Id,
            new AtualizarPedidoRequest { Titulo = "x", Descricao = "x", ValorInicial = 1m }));

        await Assert.ThrowsAsync<Domain.Exceptions.DomainException>(() => env.Pedidos.CancelarAsync(cliente.Id, pedido.Id));
    }

    [Fact]
    public async Task Apenas_o_proprietario_cancela_o_pedido()
    {
        using var env = new TestEnvironment();
        var dono = await env.RegistrarAsync(TipoUsuario.Cliente);
        var outro = await env.RegistrarAsync(TipoUsuario.Cliente);
        var pedido = await env.CriarPedidoAsync(dono.Id);

        await Assert.ThrowsAsync<ForbiddenException>(() => env.Pedidos.CancelarAsync(outro.Id, pedido.Id));

        Assert.Equal(StatusPedido.Aberto, (await env.Pedidos.ObterAsync(pedido.Id)).Status);
    }

    [Fact]
    public async Task Pedidos_fechados_nao_podem_ser_editados_nem_cancelados()
    {
        using var env = new TestEnvironment();
        var cliente = await env.RegistrarAsync(TipoUsuario.Cliente);
        var pedido = await env.CriarPedidoAsync(cliente.Id);

        // Simula o que a Etapa 2 fará (não há endpoint público para isso): fecha o pedido direto no banco.
        await ForcarStatusAsync(env, pedido.Id, StatusPedido.Fechado);

        await Assert.ThrowsAsync<Domain.Exceptions.DomainException>(() => env.Pedidos.AtualizarAsync(cliente.Id, pedido.Id,
            new AtualizarPedidoRequest { Titulo = "x", Descricao = "x", ValorInicial = 1m }));
        await Assert.ThrowsAsync<Domain.Exceptions.DomainException>(() => env.Pedidos.CancelarAsync(cliente.Id, pedido.Id));
    }

    [Fact]
    public async Task Pedidos_em_negociacao_nao_podem_ser_editados()
    {
        using var env = new TestEnvironment();
        var cliente = await env.RegistrarAsync(TipoUsuario.Cliente);
        var pedido = await env.CriarPedidoAsync(cliente.Id);
        await ForcarStatusAsync(env, pedido.Id, StatusPedido.EmNegociacao);

        await Assert.ThrowsAsync<Domain.Exceptions.DomainException>(() => env.Pedidos.AtualizarAsync(cliente.Id, pedido.Id,
            new AtualizarPedidoRequest { Titulo = "x", Descricao = "x", ValorInicial = 1m }));
    }

    [Fact]
    public async Task Consulta_de_pedido_por_id_e_inexistente()
    {
        using var env = new TestEnvironment();
        var cliente = await env.RegistrarAsync(TipoUsuario.Cliente);
        var pedido = await env.CriarPedidoAsync(cliente.Id);

        var encontrado = await env.Pedidos.ObterAsync(pedido.Id);
        Assert.Equal(pedido.Id, encontrado.Id);

        await Assert.ThrowsAsync<NotFoundException>(() => env.Pedidos.ObterAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task Listagem_aplica_filtros_de_titulo_categoria_status_e_valor()
    {
        using var env = new TestEnvironment();
        var cliente = await env.RegistrarAsync(TipoUsuario.Cliente);
        await env.CriarPedidoAsync(cliente.Id, "Redação ENEM", "Redação", 100m);
        await env.CriarPedidoAsync(cliente.Id, "Aulas de Cálculo I", "Matemática", 300m);
        await env.CriarPedidoAsync(cliente.Id, "Revisão de redação", "Redação", 50m);
        var cancelado = await env.CriarPedidoAsync(cliente.Id, "Projeto de Física", "Física", 500m);
        await env.Pedidos.CancelarAsync(cliente.Id, cancelado.Id);

        var todos = await env.Pedidos.ListarAsync(new PedidoFiltroRequest());
        Assert.Equal(4, todos.TotalItens);

        var porTitulo = await env.Pedidos.ListarAsync(new PedidoFiltroRequest { Titulo = "REDAÇÃO" });
        Assert.Equal(2, porTitulo.TotalItens);

        var porCategoria = await env.Pedidos.ListarAsync(new PedidoFiltroRequest { Categoria = "matemática" });
        Assert.Equal("Aulas de Cálculo I", Assert.Single(porCategoria.Itens).Titulo);

        var porStatus = await env.Pedidos.ListarAsync(new PedidoFiltroRequest { Status = StatusPedido.Cancelado });
        Assert.Equal("Projeto de Física", Assert.Single(porStatus.Itens).Titulo);

        var porValor = await env.Pedidos.ListarAsync(new PedidoFiltroRequest { ValorMinimo = 100m, ValorMaximo = 300m });
        Assert.Equal(2, porValor.TotalItens);

        var combinado = await env.Pedidos.ListarAsync(new PedidoFiltroRequest
        {
            Titulo = "redação",
            Status = StatusPedido.Aberto,
            ValorMaximo = 60m
        });
        Assert.Equal("Revisão de redação", Assert.Single(combinado.Itens).Titulo);
    }

    [Fact]
    public async Task Listagem_pagina_os_resultados()
    {
        using var env = new TestEnvironment();
        var cliente = await env.RegistrarAsync(TipoUsuario.Cliente);
        for (var i = 1; i <= 5; i++)
            await env.CriarPedidoAsync(cliente.Id, $"Pedido {i}", "Geral", i * 10m);

        var p1 = await env.Pedidos.ListarAsync(new PedidoFiltroRequest { Pagina = 1, TamanhoPagina = 2 });
        var p3 = await env.Pedidos.ListarAsync(new PedidoFiltroRequest { Pagina = 3, TamanhoPagina = 2 });
        var p4 = await env.Pedidos.ListarAsync(new PedidoFiltroRequest { Pagina = 4, TamanhoPagina = 2 });

        Assert.Equal(2, p1.Itens.Count);
        Assert.Equal(5, p1.TotalItens);
        Assert.Equal(3, p1.TotalPaginas);
        Assert.Single(p3.Itens);
        Assert.Empty(p4.Itens);
    }

    [Fact]
    public async Task Listagem_rejeita_parametros_invalidos()
    {
        using var env = new TestEnvironment();

        await Assert.ThrowsAsync<RequestValidationException>(() =>
            env.Pedidos.ListarAsync(new PedidoFiltroRequest { Pagina = 0 }));
        await Assert.ThrowsAsync<RequestValidationException>(() =>
            env.Pedidos.ListarAsync(new PedidoFiltroRequest { TamanhoPagina = 1000 }));
        await Assert.ThrowsAsync<RequestValidationException>(() =>
            env.Pedidos.ListarAsync(new PedidoFiltroRequest { ValorMinimo = 10m, ValorMaximo = 5m }));
    }

    [Fact]
    public async Task Meus_pedidos_retorna_somente_os_do_cliente()
    {
        using var env = new TestEnvironment();
        var a = await env.RegistrarAsync(TipoUsuario.Cliente);
        var b = await env.RegistrarAsync(TipoUsuario.Cliente);
        await env.CriarPedidoAsync(a.Id, "Do A 1");
        await env.CriarPedidoAsync(a.Id, "Do A 2");
        await env.CriarPedidoAsync(b.Id, "Do B");

        var meusA = await env.Pedidos.ListarMeusAsync(a.Id, new PedidoFiltroRequest());
        var meusB = await env.Pedidos.ListarMeusAsync(b.Id, new PedidoFiltroRequest());

        Assert.Equal(2, meusA.TotalItens);
        Assert.All(meusA.Itens, p => Assert.Equal(a.Id, p.ClienteId));
        Assert.Equal("Do B", Assert.Single(meusB.Itens).Titulo);
    }

    private static async Task ForcarStatusAsync(TestEnvironment env, Guid pedidoId, StatusPedido status)
    {
        // Simula a Etapa 2 (que controlará o status internamente) sem expor nenhum endpoint público.
        var entidade = await env.Db.Pedidos.SingleAsync(p => p.Id == pedidoId);
        env.Db.Entry(entidade).Property(nameof(Domain.Entities.Pedido.Status)).CurrentValue = status;
        await env.Db.SaveChangesAsync();
    }
}
