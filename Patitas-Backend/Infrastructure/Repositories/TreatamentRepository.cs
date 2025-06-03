using Microsoft.EntityFrameworkCore;
using Patitas_Backend.Core.Entities;
using Patitas_Backend.Core.Interfaces;
using Patitas_Backend.Infrastructure.Data;

namespace Patitas_Backend.Infrastructure.Repositories;

public class TreatamentRepository : ITreatamentRepository
{
    private readonly DataContext _context;

    public TreatamentRepository(DataContext context)
    {
        _context = context;
    }
    public async Task<IEnumerable<Treatament>> GetAllAsync(bool includeInactive = false)
    {
        if (includeInactive)
        {
            return await _context.Treataments
                .Include(t => t.Prescriptions)
                .ToListAsync();
        }
        else
        {
            return await _context.Treataments
                .Where(t => !t.IsDeleted)
                .Include(t => t.Prescriptions)
                .ToListAsync();
        }
    }

    public async Task<IEnumerable<Treatament>> GetAllDeletedAsync()
    {
        return await _context.Treataments
            .Where(t => t.IsDeleted)
            .Include(t => t.Prescriptions)
            .ToListAsync();
    }

    public async Task<Treatament?> GetByIdAsync(int id)
    {
        return await _context.Treataments
            .Where(t => !t.IsDeleted)
            .Include(t => t.Prescriptions)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Treatament?> GetDeletedByIdAsync(int id)
    {
        return await _context.Treataments
            .Where(t => t.IsDeleted)
            .Include(t => t.Prescriptions)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Treatament> CreateAsync(Treatament entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        entity.IsDeleted = false;

        _context.Treataments.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<Treatament> UpdateAsync(Treatament entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;

        _context.Entry(entity).State = EntityState.Modified;
        _context.Entry(entity).Property(x => x.CreatedAt).IsModified = false;
        _context.Entry(entity).Property(x => x.IsDeleted).IsModified = false;
        _context.Entry(entity).Property(x => x.DeletedAt).IsModified = false;
        _context.Entry(entity).Property(x => x.DeletedBy).IsModified = false;

        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> DeleteAsync(int id, string? deletedBy = null)
    {
        var entity = await _context.Treataments
            .Where(t => !t.IsDeleted)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (entity == null)
            return false;

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        entity.DeletedBy = deletedBy;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RestoreAsync(int id)
    {
        var entity = await _context.Treataments
            .Where(t => t.IsDeleted)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (entity == null)
            return false;

        entity.IsDeleted = false;
        entity.DeletedAt = null;
        entity.DeletedBy = null;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Treataments
            .AnyAsync(t => t.Id == id && !t.IsDeleted);
    }

    public async Task<bool> ExistsDeletedAsync(int id)
    {
        return await _context.Treataments
            .AnyAsync(t => t.Id == id && t.IsDeleted);
    }
}
