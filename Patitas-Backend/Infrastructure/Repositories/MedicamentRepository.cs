using Microsoft.EntityFrameworkCore;
using Patitas_Backend.Core.Entities;
using Patitas_Backend.Core.Interfaces;
using Patitas_Backend.Infrastructure.Data;

namespace Patitas_Backend.Infrastructure.Repositories;


public class MedicamentRepository : IMedicamentRepository
{
    private readonly DataContext _context;

    public MedicamentRepository(DataContext context)
    {
        _context = context;
    }

    // 1) Listar todos, con opción de incluir inactivos o no
    public async Task<IEnumerable<Medicament>> GetAllAsync(bool includeInactive = false)
    {
        if (includeInactive)
        {
            return await _context.Medicaments
                .ToListAsync();
        }

        return await _context.Medicaments
        .Where(m => !m.IsDeleted)
            .ToListAsync();
    }

    // 2) Listar solo inactivos
    public async Task<IEnumerable<Medicament>> GetAllDeletedAsync()
    {
        return await _context.Medicaments
        .Where(m => m.IsDeleted)
            .ToListAsync();
    }

    // 3) Obtener activo por Id
    public async Task<Medicament?> GetByIdAsync(int id)
    {
        return await _context.Medicaments
            .Where(m => !m.IsDeleted)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    // 4) Obtener inactivo por Id
    public async Task<Medicament?> GetDeletedByIdAsync(int id)
    {
        return await _context.Medicaments
            .Where(m => m.IsDeleted)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    // 5) Crear uno nuevo
    public async Task<Medicament> CreateAsync(Medicament medicamento)
    {
        medicamento.CreatedAt = DateTime.UtcNow;
        medicamento.IsDeleted = false;

        _context.Medicaments.Add(medicamento);
        await _context.SaveChangesAsync();
        return medicamento;
    }

    // 6) Actualizar existente (solo campos editables y UpdatedAt)
    public async Task<Medicament> UpdateAsync(Medicament medicamento)
    {
        medicamento.UpdatedAt = DateTime.UtcNow;

        // Marcar entidad como modificada
        _context.Entry(medicamento).State = EntityState.Modified;

        // Evitar sobrescribir CreatedAt e IsDeleted y campos de borrado
        _context.Entry(medicamento).Property(x => x.CreatedAt).IsModified = false;
        _context.Entry(medicamento).Property(x => x.IsDeleted).IsModified = false;
        _context.Entry(medicamento).Property(x => x.DeletedAt).IsModified = false;
        _context.Entry(medicamento).Property(x => x.DeletedBy).IsModified = false;

        await _context.SaveChangesAsync();
        return medicamento;
    }

    // 7) Inactivar (borrado lógico). Retorna false si no existe o ya está inactivo
    public async Task<bool> DeleteAsync(int id, string? deletedBy = null)
    {
        var medicamento = await _context.Medicaments
            .Where(m => !m.IsDeleted)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (medicamento == null)
            return false;

        // Aquí podrías verificar si el medicamento está siendo usado en tratamientos:
        // if (_context.Tratamientos.Any(t => t.MedicamentoId == id && !t.IsDeleted)) { return false; }

        medicamento.IsDeleted = true;
        medicamento.DeletedAt = DateTime.UtcNow;
        medicamento.DeletedBy = deletedBy;

        await _context.SaveChangesAsync();
        return true;
    }

    // 8) Restaurar uno previamente inactivado
    public async Task<bool> RestoreAsync(int id)
    {
        var medicamento = await _context.Medicaments
            .Where(m => m.IsDeleted)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (medicamento == null)
            return false;

        medicamento.IsDeleted = false;
        medicamento.DeletedAt = null;
        medicamento.DeletedBy = null;
        medicamento.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    // 9) Validar existencia activo
    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Medicaments
            .AnyAsync(m => m.Id == id && !m.IsDeleted);
    }

    // 10) Validar existencia inactivo
    public async Task<bool> ExistsDeletedAsync(int id)
    {
        return await _context.Medicaments
            .AnyAsync(m => m.Id == id && m.IsDeleted);
    }

    // 11) Validar nombre comercial duplicado (solo entre activos)
    public async Task<bool> CommercialNameExistsAsync(string commercialName, int? excludeId = null)
    {
        var query = _context.Medicaments
            .Where(m => m.CommercialName == commercialName && !m.IsDeleted);

        if (excludeId.HasValue)
            query = query.Where(m => m.Id != excludeId.Value);

        return await query.AnyAsync();
    }
}
