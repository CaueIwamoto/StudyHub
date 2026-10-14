using StudyHub.Domain.Entities;

namespace StudyHub.Application.Abstractions;

public sealed record AccessToken(string Token, DateTime ExpiraEmUtc);

public interface ITokenService
{
    AccessToken GerarToken(Usuario usuario);
}
