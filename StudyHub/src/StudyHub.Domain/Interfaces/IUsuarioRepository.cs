using StudyHub.Domain.Entities;

namespace StudyHub.Domain.Interfaces;

public interface IUsuarioRepository
{
    Task<Usuario?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Usuario?> GetByEmailAsync(string emailNormalizado, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string emailNormalizado, CancellationToken cancellationToken = default);
    Task AddAsync(Usuario usuario, CancellationToken cancellationToken = default);
}
