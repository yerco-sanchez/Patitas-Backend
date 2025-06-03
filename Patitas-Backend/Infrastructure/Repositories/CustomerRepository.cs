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
        return await _context.Customers
            .Where(c => !c.IsDeleted)
            .Include(c => c.Patients)
            .ToListAsync();
    }

    public async Task<IEnumerable<Customer>> GetAllDeletedAsync()
    {
        return await _context.Customers
            .Where(c => c.IsDeleted)
            .ToListAsync();
    }

    public async Task<Customer?> GetByIdAsync(int id)
    {
        return await _context.Customers
            .Include(c => c.Patients)
            .Where(c => !c.IsDeleted)
            .FirstOrDefaultAsync(c => c.CustomerId == id);
    }

    public async Task<Customer?> GetDeletedByIdAsync(int id)
    {
        return await _context.Customers
            .Include(c => c.Patients)
            .Where(c => c.IsDeleted)
            .FirstOrDefaultAsync(c => c.CustomerId == id);
    }

    public async Task<Customer> CreateAsync(Customer customer)
    {
        customer.CreatedAt = DateTime.UtcNow;
        customer.IsDeleted = false;

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();
        return customer;
    }

    public async Task<Customer> UpdateAsync(Customer customer)
    {
        customer.UpdatedAt = DateTime.UtcNow;

        _context.Entry(customer).State = EntityState.Modified;
        _context.Entry(customer).Property(x => x.CreatedAt).IsModified = false;
        _context.Entry(customer).Property(x => x.IsDeleted).IsModified = false;
        _context.Entry(customer).Property(x => x.DeletedAt).IsModified = false;
        _context.Entry(customer).Property(x => x.DeletedBy).IsModified = false;

        await _context.SaveChangesAsync();
        return customer;
    }

    public async Task<bool> DeleteAsync(int id, string? deletedBy = null)
    {
        var customer = await _context.Customers
            .Where(c => !c.IsDeleted)
            .FirstOrDefaultAsync(c => c.CustomerId == id);

        if (customer == null)
            return false;

        customer.IsDeleted = true;
        customer.DeletedAt = DateTime.UtcNow;
        customer.DeletedBy = deletedBy;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RestoreAsync(int id)
    {
        var customer = await _context.Customers
            .Where(c => c.IsDeleted)
            .FirstOrDefaultAsync(c => c.CustomerId == id);

        if (customer == null)
            return false;

        customer.IsDeleted = false;
        customer.DeletedAt = null;
        customer.DeletedBy = null;
        customer.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> EmailExistsAsync(string email, int? excludeCustomerId = null)
    {
        var query = _context.Customers
            .Where(c => c.Email == email && !c.IsDeleted);

        if (excludeCustomerId.HasValue)
            query = query.Where(c => c.CustomerId != excludeCustomerId.Value);

        return await query.AnyAsync();
    }

    public async Task<bool> NationalIdExistsAsync(string nationalId, int? excludeCustomerId = null)
    {
        var query = _context.Customers
            .Where(c => c.DocumentId == nationalId && !c.IsDeleted);

        if (excludeCustomerId.HasValue)
            query = query.Where(c => c.CustomerId != excludeCustomerId.Value);

        return await query.AnyAsync();
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Customers
            .AnyAsync(c => c.CustomerId == id && !c.IsDeleted);
    }

    public async Task<bool> ExistsDeletedAsync(int id)
    {
        return await _context.Customers
            .AnyAsync(c => c.CustomerId == id && c.IsDeleted);
    }
}