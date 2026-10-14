using StudyHub.Domain.Enums;

namespace StudyHub.Domain.Entities;

public class Usuario
{
    // Construtor exigido pelo EF Core.
    private Usuario()
    {
        Nome = string.Empty;
        Email = string.Empty;
        SenhaHash = string.Empty;
    }

    public Guid Id { get; private set; }
    public string Nome { get; private set; }
    public string Email { get; private set; }
    public string SenhaHash { get; private set; }
    public TipoUsuario Tipo { get; private set; }
    public string? DescricaoPerfil { get; private set; }
    public List<string> AreasConhecimento { get; private set; } = new();
    public DateTime DataCadastro { get; private set; }

    public ICollection<Pedido> Pedidos { get; private set; } = new List<Pedido>();
    public ICollection<Proposta> Propostas { get; private set; } = new List<Proposta>();

    public static Usuario Criar(
        string nome,
        string email,
        string senhaHash,
        TipoUsuario tipo,
        string? descricaoPerfil,
        IEnumerable<string>? areasConhecimento)
    {
        return new Usuario
        {
            Id = Guid.NewGuid(),
            Nome = nome,
            Email = email,
            SenhaHash = senhaHash,
            Tipo = tipo,
            DescricaoPerfil = descricaoPerfil,
            AreasConhecimento = areasConhecimento?.ToList() ?? new List<string>(),
            DataCadastro = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Atualiza somente dados de perfil. Tipo (papel), e-mail e senha NÃO são alteráveis aqui.
    /// </summary>
    public void AtualizarPerfil(string nome, string? descricaoPerfil, IEnumerable<string>? areasConhecimento)
    {
        Nome = nome;
        DescricaoPerfil = descricaoPerfil;
        AreasConhecimento = areasConhecimento?.ToList() ?? new List<string>();
    }
}
