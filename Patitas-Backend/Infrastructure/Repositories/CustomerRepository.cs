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

    public async Task<Customer?> GetByIdAsync(int id)
    {
        return await _context.Customers
            .Include(c => c.Patients)
            .FirstOrDefaultAsync(c => c.CustomerId == id);
    }

    public async Task<Customer> CreateAsync(Customer customer)
    {
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();
        return customer;
    }

    public async Task<Customer> UpdateAsync(Customer customer)
    {
        _context.Entry(customer).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return customer;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null)
            return false;

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> EmailExistsAsync(string email, int? excludeCustomerId = null)
    {
        var query = _context.Customers.Where(c => c.Email == email);
        if (excludeCustomerId.HasValue)
            query = query.Where(c => c.CustomerId != excludeCustomerId.Value);

        return await query.AnyAsync();
    }

    public async Task<bool> NationalIdExistsAsync(string nationalId, int? excludeCustomerId = null)
    {
        var query = _context.Customers.Where(c => c.NationalId == nationalId);
        if (excludeCustomerId.HasValue)
            query = query.Where(c => c.CustomerId != excludeCustomerId.Value);

        return await query.AnyAsync();
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Customers.AnyAsync(c => c.CustomerId == id);
    }
}
