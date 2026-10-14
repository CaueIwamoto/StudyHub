using System.ComponentModel.DataAnnotations;
using StudyHub.Domain.Enums;

namespace StudyHub.Application.DTOs;

public class RegisterRequest
{
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(120, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 120 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    [StringLength(254, ErrorMessage = "O e-mail deve ter no máximo 254 caracteres.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "A senha é obrigatória.")]
    [StringLength(72, MinimumLength = 8, ErrorMessage = "A senha deve ter entre 8 e 72 caracteres.")]
    public string Senha { get; set; } = string.Empty;

    /// <summary>Cliente (1) ou Prestador (2). Aceita o nome ou o número.</summary>
    [EnumDataType(typeof(TipoUsuario), ErrorMessage = "Tipo de usuário inválido. Use Cliente ou Prestador.")]
    public TipoUsuario Tipo { get; set; }

    [StringLength(1000, ErrorMessage = "A descrição do perfil deve ter no máximo 1000 caracteres.")]
    public string? DescricaoPerfil { get; set; }

    public List<string>? AreasConhecimento { get; set; }
}

public class LoginRequest
{
    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "A senha é obrigatória.")]
    public string Senha { get; set; } = string.Empty;
}

public sealed record AuthResponse(string Token, DateTime ExpiraEm, UsuarioPerfilDto Usuario);
