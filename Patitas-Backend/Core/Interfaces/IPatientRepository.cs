using Microsoft.EntityFrameworkCore;
using Patitas_Backend.Core.Entities;

namespace Patitas_Backend.Core.Interfaces;

public interface IPatientRepository
{
    Task<IEnumerable<Patient>> GetPatientsByCustomerIdAsync(int customerId);
    Task<IEnumerable<Patient>> GetAllPatientsAsync();
    Task<IEnumerable<Patient>> GetDeletedPatientsAsync();
    Task<Patient?> GetByIdAsync(int id);
    Task<Patient> CreateAsync(Patient patient);
    Task<Patient> UpdateAsync(Patient patient);
    Task<bool> ExistsAsync(int id);
    Task<bool> DeleteAsync(int id, string deletedBy = "System");
    Task<bool> RestoreAsync(int id, string restoredBy = "System");
    Task<bool> AnimalNameExistsForCustomerAsync(string animalName, int customerId, int? excludePatientId = null);
    Task<bool> UpdatePhotoAsync(int patientId, string photoUrl);
    Task<IEnumerable<string>> GetSpeciesAsync();
    Task<IEnumerable<string>> GetBreedsAsync();
    Task<(IEnumerable<Patient> Patients, int TotalCount)> SearchPatientsAsync(PatientSearchParameters parameters);
}