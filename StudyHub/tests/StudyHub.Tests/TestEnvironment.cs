using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using StudyHub.Application.DTOs;
using StudyHub.Application.Services;
using StudyHub.Domain.Enums;
using StudyHub.Infrastructure.Authentication;
using StudyHub.Infrastructure.Persistence;
using StudyHub.Infrastructure.Repositories;
using StudyHub.Infrastructure.Security;

namespace StudyHub.Tests;

/// <summary>
/// Monta serviços reais (Application + repositórios EF Core) sobre um banco InMemory isolado por teste
/// (ou sobre as opções informadas, p.ex. PostgreSQL real nos testes de integração).
/// Cada instância tem o seu próprio DbContext; várias instâncias com as mesmas opções simulam requisições distintas.
/// </summary>
public sealed class TestEnvironment : IDisposable
{
    public StudyHubDbContext Db { get; }
    public JwtSettings Jwt { get; }
    public BCryptPasswordHasher Hasher { get; }
    public AuthService Auth { get; }
    public UsuarioService Usuarios { get; }
    public PedidoService Pedidos { get; }
    public PropostaService Propostas { get; }

    public static DbContextOptions<StudyHubDbContext> NovasOpcoesInMemory() =>
        new DbContextOptionsBuilder<StudyHubDbContext>()
            .UseInMemoryDatabase($"studyhub-tests-{Guid.NewGuid()}")
            // O provider InMemory não suporta transações; o fluxo de negócio as usa (ExecuteInTransactionAsync).
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

    public TestEnvironment(DbContextOptions<StudyHubDbContext>? options = null)
    {
        options ??= NovasOpcoesInMemory();

        Db = new StudyHubDbContext(options);
        Jwt = new JwtSettings
        {
            SecretKey = new string('k', 48),
            Issuer = "studyhub-tests",
            Audience = "studyhub-tests-clients",
            ExpirationMinutes = 30
        };

        var usuarios = new UsuarioRepository(Db);
        var pedidos = new PedidoRepository(Db);
        var propostas = new PropostaRepository(Db);
        var unitOfWork = new UnitOfWork(Db);

        Hasher = new BCryptPasswordHasher(workFactor: 4); // custo mínimo: testes rápidos
        var tokens = new JwtTokenService(Jwt);

        Auth = new AuthService(usuarios, unitOfWork, Hasher, tokens);
        Usuarios = new UsuarioService(usuarios, unitOfWork);
        Pedidos = new PedidoService(pedidos, propostas, usuarios, unitOfWork);
        Propostas = new PropostaService(propostas, pedidos, usuarios, unitOfWork);
    }

    public const string SenhaPadrao = "Senha@1234";

    public Task<UsuarioPerfilDto> RegistrarAsync(TipoUsuario tipo, string? email = null, string nome = "Fulano de Tal")
        => Auth.RegisterAsync(new RegisterRequest
        {
            Nome = nome,
            Email = email ?? $"{Guid.NewGuid():N}@teste.com",
            Senha = SenhaPadrao,
            Tipo = tipo
        });

    public Task<PedidoDto> CriarPedidoAsync(
        Guid clienteId,
        string titulo = "Redação de português ENEM",
        string? categoria = "Redação",
        decimal valor = 100m)
        => Pedidos.CriarAsync(clienteId, new CriarPedidoRequest
        {
            Titulo = titulo,
            Descricao = "Preciso de orientação e revisão de uma redação sobre o tema Discriminação Ideológica.",
            Categoria = categoria,
            ValorInicial = valor
        });

    public Task<PropostaResponse> EnviarPropostaAsync(
        Guid prestadorId,
        Guid pedidoId,
        TipoProposta tipo,
        decimal valor,
        string? mensagem = "Tenho experiência com redações do ENEM e posso orientar e revisar o trabalho.")
        => Propostas.CriarAsync(prestadorId, pedidoId, new CriarPropostaRequest
        {
            Tipo = tipo,
            ValorProposto = valor,
            Mensagem = mensagem
        });

    public void Dispose() => Db.Dispose();
}
