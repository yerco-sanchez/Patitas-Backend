using Patitas_Backend.Core.Entities;

namespace Patitas_Backend.Core.Interfaces;

public interface ITreatamentRepository
{
    Task<IEnumerable<Treatament>> GetAllAsync(bool includeInactive = false);
    Task<IEnumerable<Treatament>> GetAllDeletedAsync();
    Task<Treatament?> GetByIdAsync(int id);
    Task<Treatament?> GetDeletedByIdAsync(int id);
    Task<Treatament> CreateAsync(Treatament entity);
    Task<Treatament> UpdateAsync(Treatament entity);
    Task<bool> DeleteAsync(int id, string? deletedBy = null);
    Task<bool> RestoreAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> ExistsDeletedAsync(int id);
}
