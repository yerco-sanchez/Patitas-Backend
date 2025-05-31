using Patitas_Backend.Core.Entities;

namespace Patitas_Backend.Core.Interfaces;

public interface ICustomerRepository
{
    Task<IEnumerable<Customer>> GetAllAsync();
}
