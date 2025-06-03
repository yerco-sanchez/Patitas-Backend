using Patitas_Backend.Core.Entities;

namespace Patitas_Backend.Core.Interfaces;

public interface ICustomerRepository
{
    Task<IEnumerable<Customer>> GetAllAsync();
    Task<IEnumerable<Customer>> GetAllDeletedAsync();
    Task<Customer?> GetByIdAsync(int id);
    Task<Customer?> GetDeletedByIdAsync(int id);
    Task<Customer> CreateAsync(Customer customer);
    Task<Customer> UpdateAsync(Customer customer);
    Task<bool> DeleteAsync(int id, string? deletedBy = null);
    Task<bool> RestoreAsync(int id);
    Task<bool> EmailExistsAsync(string email, int? excludeCustomerId = null);
    Task<bool> NationalIdExistsAsync(string nationalId, int? excludeCustomerId = null);
    Task<bool> ExistsAsync(int id);
    Task<bool> ExistsDeletedAsync(int id);
}
