using Microsoft.EntityFrameworkCore;
using Patitas_Backend.Core.Entities;
using Patitas_Backend.Core.Interfaces;
using Patitas_Backend.Infrastructure.Data;

namespace Patitas_Backend.Infrastructure.Repositories;

public class MedicamentPrescriptionRepository : IMedicamentPrescriptionRepository
{
    private readonly DataContext _context;

    public MedicamentPrescriptionRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<MedicamentPrescription>> GetAllAsync(bool includeInactive = false)
    {
        if (includeInactive)
        {
            return await _context.MedicamentPrescriptions
                .Include(mp => mp.Treatament)
                .Include(mp => mp.Medicament)
                .ToListAsync();
        }
        else
        {
            return await _context.MedicamentPrescriptions
                .Where(mp => !mp.IsDeleted)
                .Include(mp => mp.Treatament)
                .Include(mp => mp.Medicament)
                .ToListAsync();
        }
    }

    public async Task<IEnumerable<MedicamentPrescription>> GetAllDeletedAsync()
    {
        return await _context.MedicamentPrescriptions
            .Where(mp => mp.IsDeleted)
            .Include(mp => mp.Treatament)
            .Include(mp => mp.Medicament)
            .ToListAsync();
    }

    public async Task<MedicamentPrescription?> GetByIdAsync(int id)
    {
        return await _context.MedicamentPrescriptions
            .Where(mp => !mp.IsDeleted)
            .Include(mp => mp.Treatament)
            .Include(mp => mp.Medicament)
            .FirstOrDefaultAsync(mp => mp.Id == id);
    }

    public async Task<MedicamentPrescription?> GetDeletedByIdAsync(int id)
    {
        return await _context.MedicamentPrescriptions
            .Where(mp => mp.IsDeleted)
            .Include(mp => mp.Treatament)
            .Include(mp => mp.Medicament)
            .FirstOrDefaultAsync(mp => mp.Id == id);
    }

    public async Task<MedicamentPrescription> CreateAsync(MedicamentPrescription entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        entity.IsDeleted = false;

        _context.MedicamentPrescriptions.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<MedicamentPrescription> UpdateAsync(MedicamentPrescription entity)
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
        var entity = await _context.MedicamentPrescriptions
            .Where(mp => !mp.IsDeleted)
            .FirstOrDefaultAsync(mp => mp.Id == id);

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
        var entity = await _context.MedicamentPrescriptions
            .Where(mp => mp.IsDeleted)
            .FirstOrDefaultAsync(mp => mp.Id == id);

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
        return await _context.MedicamentPrescriptions
            .AnyAsync(mp => mp.Id == id && !mp.IsDeleted);
    }

    public async Task<bool> ExistsDeletedAsync(int id)
    {
        return await _context.MedicamentPrescriptions
            .AnyAsync(mp => mp.Id == id && mp.IsDeleted);
    }
}
