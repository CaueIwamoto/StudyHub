using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore;
using StudyHub.Application.DTOs;
using StudyHub.Application.Exceptions;
using StudyHub.Application.Security;
using StudyHub.Domain.Enums;

namespace StudyHub.Tests;

public class UsuarioTests
{
    [Fact]
    public async Task Cadastro_valido_cria_usuario_com_senha_em_hash_BCrypt()
    {
        using var env = new TestEnvironment();

        var perfil = await env.Auth.RegisterAsync(new RegisterRequest
        {
            Nome = "  Maria Silva ",
            Email = "Maria@Teste.com",
            Senha = TestEnvironment.SenhaPadrao,
            Tipo = TipoUsuario.Prestador,
            DescricaoPerfil = "Professora de português",
            AreasConhecimento = new List<string> { "Redação", "redação", "Gramática", " " }
        });

        Assert.NotEqual(Guid.Empty, perfil.Id);
        Assert.Equal("Maria Silva", perfil.Nome);
        Assert.Equal("maria@teste.com", perfil.Email);
        Assert.Equal(TipoUsuario.Prestador, perfil.Tipo);
        Assert.Equal(new[] { "Redação", "Gramática" }, perfil.AreasConhecimento);

        var salvo = await env.Db.Usuarios.SingleAsync();
        Assert.NotEqual(TestEnvironment.SenhaPadrao, salvo.SenhaHash);
        Assert.StartsWith("$2", salvo.SenhaHash);
        Assert.True(BCrypt.Net.BCrypt.Verify(TestEnvironment.SenhaPadrao, salvo.SenhaHash));
        Assert.Equal(DateTimeKind.Utc, salvo.DataCadastro.Kind);
    }

    [Fact]
    public void Respostas_de_usuario_nao_expoem_SenhaHash()
    {
        Assert.DoesNotContain(typeof(UsuarioPerfilDto).GetProperties(), p => p.Name == "SenhaHash");
        Assert.DoesNotContain(typeof(UsuarioPublicoDto).GetProperties(), p => p.Name == "SenhaHash");
        Assert.DoesNotContain(typeof(AuthResponse).GetProperties(), p => p.Name == "SenhaHash");
    }

    [Fact]
    public async Task Cadastro_com_email_duplicado_lanca_conflito_mesmo_com_caixa_diferente()
    {
        using var env = new TestEnvironment();
        await env.RegistrarAsync(TipoUsuario.Cliente, "dup@teste.com");

        await Assert.ThrowsAsync<ConflictException>(() =>
            env.RegistrarAsync(TipoUsuario.Prestador, "DUP@teste.com"));

        Assert.Equal(1, await env.Db.Usuarios.CountAsync());
    }

    [Theory]
    [InlineData("", "a@b.com", "Senha@1234")]
    [InlineData("Fulano", "nao-e-email", "Senha@1234")]
    [InlineData("Fulano", "a@b.com", "curta")]
    public async Task Cadastro_com_dados_invalidos_lanca_erro_de_validacao(string nome, string email, string senha)
    {
        using var env = new TestEnvironment();

        await Assert.ThrowsAsync<RequestValidationException>(() => env.Auth.RegisterAsync(new RegisterRequest
        {
            Nome = nome,
            Email = email,
            Senha = senha,
            Tipo = TipoUsuario.Cliente
        }));
    }

    [Fact]
    public async Task Cadastro_com_tipo_invalido_lanca_erro_de_validacao()
    {
        using var env = new TestEnvironment();

        await Assert.ThrowsAsync<RequestValidationException>(() => env.Auth.RegisterAsync(new RegisterRequest
        {
            Nome = "Fulano",
            Email = "a@b.com",
            Senha = TestEnvironment.SenhaPadrao,
            Tipo = (TipoUsuario)99
        }));
    }

    [Fact]
    public async Task Login_valido_retorna_JWT_com_id_nome_email_e_role()
    {
        using var env = new TestEnvironment();
        var usuario = await env.RegistrarAsync(TipoUsuario.Cliente, "login@teste.com", "Joana");

        var resposta = await env.Auth.LoginAsync(new LoginRequest { Email = "LOGIN@teste.com", Senha = TestEnvironment.SenhaPadrao });

        Assert.False(string.IsNullOrWhiteSpace(resposta.Token));
        Assert.True(resposta.ExpiraEm > DateTime.UtcNow);
        Assert.Equal(usuario.Id, resposta.Usuario.Id);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(resposta.Token);
        Assert.Equal(usuario.Id.ToString(), jwt.Claims.Single(c => c.Type == ClaimNames.UserId).Value);
        Assert.Equal("Joana", jwt.Claims.Single(c => c.Type == ClaimNames.Name).Value);
        Assert.Equal("login@teste.com", jwt.Claims.Single(c => c.Type == ClaimNames.Email).Value);
        Assert.Equal("Cliente", jwt.Claims.Single(c => c.Type == ClaimNames.Role).Value);
        Assert.Equal(env.Jwt.Issuer, jwt.Issuer);
        Assert.Contains(env.Jwt.Audience, jwt.Audiences);
    }

    [Fact]
    public async Task Login_com_senha_errada_ou_email_inexistente_lanca_nao_autorizado()
    {
        using var env = new TestEnvironment();
        await env.RegistrarAsync(TipoUsuario.Cliente, "ok@teste.com");

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            env.Auth.LoginAsync(new LoginRequest { Email = "ok@teste.com", Senha = "senha-errada" }));

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            env.Auth.LoginAsync(new LoginRequest { Email = "naoexiste@teste.com", Senha = TestEnvironment.SenhaPadrao }));
    }

    [Fact]
    public async Task Atualizar_perfil_altera_somente_dados_de_perfil_e_preserva_papel_e_email()
    {
        using var env = new TestEnvironment();
        var usuario = await env.RegistrarAsync(TipoUsuario.Cliente, "perfil@teste.com");

        var atualizado = await env.Usuarios.AtualizarPerfilAsync(usuario.Id, new AtualizarPerfilRequest
        {
            Nome = "Novo Nome",
            DescricaoPerfil = "Nova descrição",
            AreasConhecimento = new List<string> { "Matemática" }
        });

        Assert.Equal("Novo Nome", atualizado.Nome);
        Assert.Equal("Nova descrição", atualizado.DescricaoPerfil);
        Assert.Equal(new[] { "Matemática" }, atualizado.AreasConhecimento);
        Assert.Equal(TipoUsuario.Cliente, atualizado.Tipo);
        Assert.Equal("perfil@teste.com", atualizado.Email);

        // O DTO de atualização não oferece como alterar papel, e-mail ou senha.
        var campos = typeof(AtualizarPerfilRequest).GetProperties().Select(p => p.Name).ToList();
        Assert.DoesNotContain("Tipo", campos);
        Assert.DoesNotContain("Role", campos);
        Assert.DoesNotContain("Email", campos);
        Assert.DoesNotContain("Senha", campos);
    }

    [Fact]
    public async Task Perfil_publico_nao_expoe_dados_sensiveis()
    {
        using var env = new TestEnvironment();
        var usuario = await env.RegistrarAsync(TipoUsuario.Prestador);

        var publico = await env.Usuarios.ObterPublicoAsync(usuario.Id);

        Assert.Equal(usuario.Id, publico.Id);
        var campos = typeof(UsuarioPublicoDto).GetProperties().Select(p => p.Name).ToList();
        Assert.DoesNotContain("Email", campos);
        Assert.DoesNotContain("SenhaHash", campos);
        Assert.DoesNotContain("Senha", campos);
    }

    [Fact]
    public async Task Consultar_usuario_inexistente_lanca_nao_encontrado()
    {
        using var env = new TestEnvironment();

        await Assert.ThrowsAsync<NotFoundException>(() => env.Usuarios.ObterPublicoAsync(Guid.NewGuid()));
        await Assert.ThrowsAsync<NotFoundException>(() => env.Usuarios.ObterPerfilAsync(Guid.NewGuid()));
    }

    [Fact]
    public void Nomes_das_roles_coincidem_com_o_enum_TipoUsuario()
    {
        Assert.Equal(TipoUsuario.Cliente.ToString(), Papeis.Cliente);
        Assert.Equal(TipoUsuario.Prestador.ToString(), Papeis.Prestador);
    }
}
