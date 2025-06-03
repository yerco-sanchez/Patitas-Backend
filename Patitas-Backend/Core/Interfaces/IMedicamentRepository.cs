using Patitas_Backend.Core.Entities;

namespace Patitas_Backend.Core.Interfaces;

public interface IMedicamentRepository
{
        Task<IEnumerable<Medicament>> GetAllAsync(bool includeInactive = false);
        Task<IEnumerable<Medicament>> GetAllDeletedAsync();
        Task<Medicament?> GetByIdAsync(int id);
        Task<Medicament?> GetDeletedByIdAsync(int id);
        Task<Medicament> CreateAsync(Medicament medicamento);
        Task<Medicament> UpdateAsync(Medicament medicamento);
        Task<bool> DeleteAsync(int id, string? deletedBy = null);
        Task<bool> RestoreAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> ExistsDeletedAsync(int id);
        Task<bool> CommercialNameExistsAsync(string commercialName, int? excludeId = null);
}
