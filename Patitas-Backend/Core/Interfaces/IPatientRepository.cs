using Patitas_Backend.Core.Entities;

namespace Patitas_Backend.Core.Interfaces;

public interface IPatientRepository
{
    Task<Patient> CreateAsync(Patient patient);
    Task<bool> AnimalNameExistsForCustomerAsync(string animalName, int customerId, int? excludePatientId = null);

}
