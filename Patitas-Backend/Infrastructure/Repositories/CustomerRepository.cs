using Microsoft.EntityFrameworkCore;
using Patitas_Backend.Core.Entities;
using Patitas_Backend.Core.Interfaces;
using Patitas_Backend.Infrastructure.Data;

namespace Patitas_Backend.Infrastructure.Repositories;
public class CustomerRepository : ICustomerRepository
{
    private readonly DataContext _context;

    public CustomerRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Customer>> GetAllAsync()
    {
        return await _context.Customers.ToListAsync();
    }

}
