using Microsoft.EntityFrameworkCore;
using StudyHub.Domain.Entities;
using StudyHub.Domain.Interfaces;
using StudyHub.Infrastructure.Persistence;

namespace StudyHub.Infrastructure.Repositories;

public sealed class UsuarioRepository : IUsuarioRepository
{
    private readonly StudyHubDbContext _db;

    public UsuarioRepository(StudyHubDbContext db) => _db = db;

    public Task<Usuario?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _db.Usuarios.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public Task<Usuario?> GetByEmailAsync(string emailNormalizado, CancellationToken cancellationToken = default)
        => _db.Usuarios.FirstOrDefaultAsync(u => u.Email == emailNormalizado, cancellationToken);

    public Task<bool> EmailExistsAsync(string emailNormalizado, CancellationToken cancellationToken = default)
        => _db.Usuarios.AnyAsync(u => u.Email == emailNormalizado, cancellationToken);

    public async Task AddAsync(Usuario usuario, CancellationToken cancellationToken = default)
        => await _db.Usuarios.AddAsync(usuario, cancellationToken);
}
