using Patitas_Backend.Core.Entities;

namespace Patitas_Backend.Core.Interfaces;

public interface IPatientRepository
{
    Task<IEnumerable<Patient>> GetPatientsByCustomerIdAsync(int customerId);
    Task<Patient> CreateAsync(Patient patient);
    Task<Patient> UpdateAsync(Patient patient);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> AnimalNameExistsForCustomerAsync(string animalName, int customerId, int? excludePatientId = null);
    Task<bool> UpdatePhotoAsync(int patientId, string photoUrl);

}
