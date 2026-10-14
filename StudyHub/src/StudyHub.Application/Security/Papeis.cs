namespace StudyHub.Application.Security;

/// <summary>Nomes das roles usadas no JWT e em [Authorize(Roles = ...)]. Devem coincidir com os nomes de TipoUsuario.</summary>
public static class Papeis
{
    public const string Cliente = "Cliente";
    public const string Prestador = "Prestador";
}
