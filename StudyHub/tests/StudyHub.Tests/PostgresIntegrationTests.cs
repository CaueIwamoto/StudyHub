using Microsoft.EntityFrameworkCore;
using Npgsql;
using StudyHub.Application.Exceptions;
using StudyHub.Domain.Enums;
using StudyHub.Domain.Exceptions;
using StudyHub.Infrastructure.Persistence;
using StudyHub.Infrastructure.Persistence.Configurations;

namespace StudyHub.Tests;

/// <summary>
/// Fact que só executa quando a variável de ambiente STUDYHUB_TEST_POSTGRES contém uma connection string
/// de um PostgreSQL de testes (ex.: "Host=localhost;Port=5432;Database=studyhub_tests;Username=studyhub;Password=...").
/// Sem a variável, o teste aparece como "Skipped" (nunca como aprovado).
/// </summary>
public sealed class PostgresFactAttribute : FactAttribute
{
    public const string VariavelDeAmbiente = "STUDYHUB_TEST_POSTGRES";

    public PostgresFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(VariavelDeAmbiente)))
            Skip = $"Defina {VariavelDeAmbiente} com a connection string de um PostgreSQL de testes para executar.";
    }
}

/// <summary>
/// Testes de integração contra um PostgreSQL real: aplicam as migrations, exercitam o bloqueio de linha
/// (SELECT ... FOR UPDATE), a transação e os índices únicos parciais.
/// </summary>
public class PostgresIntegrationTests
{
    private static DbContextOptions<StudyHubDbContext> Opcoes() =>
        new DbContextOptionsBuilder<StudyHubDbContext>()
            .UseNpgsql(Environment.GetEnvironmentVariable(PostgresFactAttribute.VariavelDeAmbiente))
            .Options;

    private static async Task<Exception?> TentarAsync(Func<Task> acao)
    {
        try
        {
            await acao();
            return null;
        }
        catch (Exception ex)
        {
            return ex;
        }
    }

    [PostgresFact]
    public async Task Duas_tentativas_simultaneas_de_aceitar_propostas_diferentes_fecham_o_pedido_com_apenas_uma()
    {
        var options = Opcoes();
        await using (var migracao = new StudyHubDbContext(options))
            await migracao.Database.MigrateAsync(); // valida também a migration AdicionarProposta

        using var preparo = new TestEnvironment(options);
        var cliente = await preparo.RegistrarAsync(TipoUsuario.Cliente);
        var prestadorA = await preparo.RegistrarAsync(TipoUsuario.Prestador);
        var prestadorB = await preparo.RegistrarAsync(TipoUsuario.Prestador);
        var pedido = await preparo.CriarPedidoAsync(cliente.Id, valor: 100m);
        var pA = await preparo.EnviarPropostaAsync(prestadorA.Id, pedido.Id, TipoProposta.Contraproposta, 150m);
        var pB = await preparo.EnviarPropostaAsync(prestadorB.Id, pedido.Id, TipoProposta.Contraproposta, 120m);

        // Duas requisições independentes (cada uma com seu DbContext e sua conexão), disparadas ao mesmo tempo.
        using var requisicaoA = new TestEnvironment(options);
        using var requisicaoB = new TestEnvironment(options);
        var largada = new TaskCompletionSource();
        var tarefaA = Task.Run(async () => { await largada.Task; return await TentarAsync(() => requisicaoA.Propostas.AceitarAsync(cliente.Id, pA.Id)); });
        var tarefaB = Task.Run(async () => { await largada.Task; return await TentarAsync(() => requisicaoB.Propostas.AceitarAsync(cliente.Id, pB.Id)); });
        largada.SetResult();
        var erros = await Task.WhenAll(tarefaA, tarefaB);

        Assert.Equal(1, erros.Count(e => e is null));
        Assert.Equal(1, erros.Count(e => e is DomainException or ConflictException));

        using var verificacao = new TestEnvironment(options);
        var propostas = await verificacao.Db.Propostas.AsNoTracking().Where(p => p.PedidoId == pedido.Id).ToListAsync();
        var aceita = Assert.Single(propostas, p => p.Status == StatusProposta.Aceita);
        Assert.Single(propostas, p => p.Status == StatusProposta.NaoSelecionada);

        var final = await verificacao.Pedidos.ObterAsync(pedido.Id);
        Assert.Equal(StatusPedido.Fechado, final.Status);
        Assert.Equal(aceita.ValorProposto, final.ValorAcordado);
        Assert.Equal(100m, final.ValorInicial);
    }

    [PostgresFact]
    public async Task Indices_unicos_parciais_impedem_proposta_pendente_duplicada_e_duas_aceitas()
    {
        var options = Opcoes();
        await using (var migracao = new StudyHubDbContext(options))
            await migracao.Database.MigrateAsync();

        using var env = new TestEnvironment(options);
        var cliente = await env.RegistrarAsync(TipoUsuario.Cliente);
        var prestador = await env.RegistrarAsync(TipoUsuario.Prestador);
        var pedido = await env.CriarPedidoAsync(cliente.Id);
        await env.EnviarPropostaAsync(prestador.Id, pedido.Id, TipoProposta.Contraproposta, 150m);

        // Ignora a verificação do serviço e força a violação direto no banco.
        await using var db = new StudyHubDbContext(options);
        var ex = await Assert.ThrowsAsync<PostgresException>(() => db.Database.ExecuteSqlInterpolatedAsync($@"
            INSERT INTO ""Propostas"" (""Id"", ""PedidoId"", ""PrestadorId"", ""ValorProposto"", ""Tipo"", ""Status"", ""DataCriacao"")
            VALUES ({Guid.NewGuid()}, {pedido.Id}, {prestador.Id}, 160.00, 2, 1, now())"));

        Assert.Equal(PostgresErrorCodes.UniqueViolation, ex.SqlState);
        Assert.Equal(PropostaConfiguration.IndicePendentePorPrestador, ex.ConstraintName);
    }
}
