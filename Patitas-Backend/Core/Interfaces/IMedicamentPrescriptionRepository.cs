using Patitas_Backend.Core.Entities;

namespace Patitas_Backend.Core.Interfaces;

public interface IMedicamentPrescriptionRepository
{
    Task<IEnumerable<MedicamentPrescription>> GetAllAsync(bool includeInactive = false);
    Task<IEnumerable<MedicamentPrescription>> GetAllDeletedAsync();
    Task<MedicamentPrescription?> GetByIdAsync(int id);
    Task<MedicamentPrescription?> GetDeletedByIdAsync(int id);
    Task<MedicamentPrescription> CreateAsync(MedicamentPrescription entity);
    Task<MedicamentPrescription> UpdateAsync(MedicamentPrescription entity);
    Task<bool> DeleteAsync(int id, string? deletedBy = null);
    Task<bool> RestoreAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> ExistsDeletedAsync(int id);
}
