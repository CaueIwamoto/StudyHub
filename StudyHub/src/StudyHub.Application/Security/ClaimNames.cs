namespace StudyHub.Application.Security;

/// <summary>Nomes (curtos) das claims emitidas no JWT.</summary>
public static class ClaimNames
{
    public const string UserId = "sub";
    public const string Name = "name";
    public const string Email = "email";
    public const string Role = "role";
}
