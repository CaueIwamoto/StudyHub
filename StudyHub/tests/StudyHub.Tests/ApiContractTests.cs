using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Routing;
using StudyHub.Api.Controllers;
using StudyHub.Application.Security;

namespace StudyHub.Tests;

/// <summary>Verifica, por reflexão, as regras de autorização declaradas nos controllers.</summary>
public class ApiContractTests
{
    [Fact]
    public void Criar_pedido_exige_papel_Cliente()
    {
        var metodo = typeof(PedidosController).GetMethod(nameof(PedidosController.Criar))!;
        var auth = metodo.GetCustomAttributes<AuthorizeAttribute>(inherit: true).Single();
        Assert.Equal(Papeis.Cliente, auth.Roles);
    }

    [Theory]
    [InlineData(nameof(PedidosController.Atualizar))]
    [InlineData(nameof(PedidosController.Cancelar))]
    [InlineData(nameof(PedidosController.ListarMeus))]
    public void Acoes_de_cliente_exigem_papel_Cliente(string acao)
    {
        var metodo = typeof(PedidosController).GetMethod(acao)!;
        var auth = metodo.GetCustomAttributes<AuthorizeAttribute>(inherit: true).Single();
        Assert.Equal(Papeis.Cliente, auth.Roles);
    }

    [Fact]
    public void Controllers_protegidos_exigem_autenticacao()
    {
        Assert.NotEmpty(typeof(PedidosController).GetCustomAttributes<AuthorizeAttribute>());
        Assert.NotEmpty(typeof(UsuariosController).GetCustomAttributes<AuthorizeAttribute>());
        Assert.NotEmpty(typeof(AuthController).GetCustomAttributes<AllowAnonymousAttribute>());
    }

    [Fact]
    public void Nao_existe_endpoint_publico_para_alterar_status_do_pedido()
    {
        var templates = typeof(PedidosController)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .SelectMany(m => m.GetCustomAttributes<HttpMethodAttribute>())
            .Select(a => a.Template ?? string.Empty)
            .ToList();

        Assert.DoesNotContain(templates, t =>
            t.Contains("status", StringComparison.OrdinalIgnoreCase) ||
            t.Contains("fechar", StringComparison.OrdinalIgnoreCase) ||
            t.Contains("negociacao", StringComparison.OrdinalIgnoreCase));
        Assert.Contains("{id:guid}/cancelar", templates);
    }

    [Theory]
    [InlineData(nameof(PropostasController.Criar), Papeis.Prestador)]
    [InlineData(nameof(PropostasController.ListarMinhas), Papeis.Prestador)]
    [InlineData(nameof(PropostasController.ListarPorPedido), Papeis.Cliente)]
    [InlineData(nameof(PropostasController.Aceitar), Papeis.Cliente)]
    [InlineData(nameof(PropostasController.Recusar), Papeis.Cliente)]
    public void Acoes_de_proposta_exigem_o_papel_correto(string acao, string papel)
    {
        var metodo = typeof(PropostasController).GetMethod(acao)!;
        var auth = metodo.GetCustomAttributes<AuthorizeAttribute>(inherit: true).Single();
        Assert.Equal(papel, auth.Roles);
    }

    [Fact]
    public void Controller_de_propostas_exige_autenticacao_e_expoe_somente_as_rotas_previstas()
    {
        Assert.NotEmpty(typeof(PropostasController).GetCustomAttributes<AuthorizeAttribute>());

        var rotas = typeof(PropostasController)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .SelectMany(m => m.GetCustomAttributes<HttpMethodAttribute>())
            .Select(a => $"{a.HttpMethods.Single()} {a.Template}")
            .OrderBy(x => x)
            .ToList();

        var esperadas = new[]
        {
            "GET pedidos/{pedidoId:guid}/propostas",
            "GET propostas/minhas",
            "GET propostas/{id:guid}",
            "PATCH propostas/{id:guid}/aceitar",
            "PATCH propostas/{id:guid}/recusar",
            "POST pedidos/{pedidoId:guid}/propostas"
        }.OrderBy(x => x).ToList();

        Assert.Equal(esperadas, rotas); // nenhum endpoint genérico para alterar status
    }

    [Fact]
    public void Listagem_de_pedidos_disponiveis_exige_papel_Prestador()
    {
        var metodo = typeof(PedidosController).GetMethod(nameof(PedidosController.ListarDisponiveis))!;
        var auth = metodo.GetCustomAttributes<AuthorizeAttribute>(inherit: true).Single();
        Assert.Equal(Papeis.Prestador, auth.Roles);
    }
}
