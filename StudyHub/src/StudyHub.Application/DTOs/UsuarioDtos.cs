using System.ComponentModel.DataAnnotations;
using StudyHub.Domain.Enums;

namespace StudyHub.Application.DTOs;

/// <summary>Perfil completo, visível apenas para o próprio usuário. Nunca contém a senha/hash.</summary>
public sealed record UsuarioPerfilDto(
    Guid Id,
    string Nome,
    string Email,
    TipoUsuario Tipo,
    string? DescricaoPerfil,
    IReadOnlyList<string> AreasConhecimento,
    DateTime DataCadastro);

/// <summary>Perfil público: sem e-mail e sem qualquer dado sensível.</summary>
public sealed record UsuarioPublicoDto(
    Guid Id,
    string Nome,
    TipoUsuario Tipo,
    string? DescricaoPerfil,
    IReadOnlyList<string> AreasConhecimento,
    DateTime DataCadastro);

/// <summary>Somente campos de perfil. Não existe campo Tipo/Role/Email: o papel não pode ser alterado por aqui.</summary>
public class AtualizarPerfilRequest
{
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(120, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 120 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "A descrição do perfil deve ter no máximo 1000 caracteres.")]
    public string? DescricaoPerfil { get; set; }

    public List<string>? AreasConhecimento { get; set; }
}
