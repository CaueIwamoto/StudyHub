namespace StudyHub.Infrastructure.Authentication;

public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = "StudyHub";
    public string Audience { get; set; } = "StudyHub.Clients";
    public int ExpirationMinutes { get; set; } = 60;

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(SecretKey) || SecretKey.Length < 32)
            throw new InvalidOperationException(
                "Jwt:SecretKey não configurada ou muito curta (mínimo de 32 caracteres). Defina Jwt__SecretKey no .env ou nas variáveis de ambiente.");
        if (string.IsNullOrWhiteSpace(Issuer))
            throw new InvalidOperationException("Jwt:Issuer é obrigatório.");
        if (string.IsNullOrWhiteSpace(Audience))
            throw new InvalidOperationException("Jwt:Audience é obrigatório.");
        if (ExpirationMinutes <= 0)
            throw new InvalidOperationException("Jwt:ExpirationMinutes deve ser maior que zero.");
    }
}
