using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using StudyHub.Application.Abstractions;
using StudyHub.Application.Security;
using StudyHub.Domain.Entities;

namespace StudyHub.Infrastructure.Authentication;

public sealed class JwtTokenService : ITokenService
{
    private readonly JwtSettings _settings;

    public JwtTokenService(JwtSettings settings)
    {
        settings.Validate();
        _settings = settings;
    }

    public AccessToken GerarToken(Usuario usuario)
    {
        var agora = DateTime.UtcNow;
        var expira = agora.AddMinutes(_settings.ExpirationMinutes);

        var claims = new List<Claim>
        {
            new(ClaimNames.UserId, usuario.Id.ToString()),
            new("jti", Guid.NewGuid().ToString()),
            new(ClaimNames.Name, usuario.Nome),
            new(ClaimNames.Email, usuario.Email),
            new(ClaimNames.Role, usuario.Tipo.ToString())
        };

        var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
        var credenciais = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            notBefore: agora,
            expires: expira,
            signingCredentials: credenciais);

        return new AccessToken(new JwtSecurityTokenHandler().WriteToken(token), expira);
    }
}
