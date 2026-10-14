using StudyHub.Application.Abstractions;

namespace StudyHub.Infrastructure.Security;

public sealed class BCryptPasswordHasher : IPasswordHasher
{
    private readonly int _workFactor;

    public BCryptPasswordHasher(int workFactor = 11)
    {
        _workFactor = workFactor;
    }

    public string Hash(string senha) => BCrypt.Net.BCrypt.HashPassword(senha, _workFactor);

    public bool Verify(string senha, string hash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(senha, hash);
        }
        catch (Exception)
        {
            // Hash malformado: trata como senha inválida.
            return false;
        }
    }
}
