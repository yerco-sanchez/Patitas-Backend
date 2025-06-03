using Microsoft.EntityFrameworkCore;
using Patitas_Backend.Core.Entities;
using Patitas_Backend.Core.Enumerables;
using Patitas_Backend.Core.Interfaces;
using Patitas_Backend.Infrastructure.Data;

namespace Patitas_Backend.Infrastructure.Repositories;

public class PatientRepository : IPatientRepository
{
    private readonly DataContext _context;

    public PatientRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Patient>> GetPatientsByCustomerIdAsync(int customerId)
    {
        return await _context.Patients
            .Where(p => p.CustomerId == customerId && !p.IsDeleted)
            .Include(p => p.Customer)
            .OrderBy(p => p.AnimalName)
            .ToListAsync();
    }

    public async Task<IEnumerable<Patient>> GetAllPatientsAsync()
    {
        return await _context.Patients
            .Where(p => !p.IsDeleted)
            .Include(p => p.Customer)
            .OrderBy(p => p.AnimalName)
            .ToListAsync();
    }

    public async Task<IEnumerable<Patient>> GetDeletedPatientsAsync()
    {
        return await _context.Patients
            .Where(p => p.IsDeleted)
            .Include(p => p.Customer)
            .OrderBy(p => p.DeletedAt)
            .ToListAsync();
    }

    public async Task<Patient?> GetByIdAsync(int id)
    {
        return await _context.Patients
            .Include(p => p.Customer)
            .FirstOrDefaultAsync(p => p.PatientId == id && !p.IsDeleted);
    }

    public async Task<Patient> CreateAsync(Patient patient)
    {
        patient.RegisteredAt = DateTime.UtcNow;
        patient.IsDeleted = false;
        _context.Patients.Add(patient);
        await _context.SaveChangesAsync();
        return patient;
    }

    public async Task<Patient> UpdateAsync(Patient patient)
    {
        patient.UpdatedAt = DateTime.UtcNow;
        _context.Entry(patient).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return patient;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Patients.AnyAsync(p => p.PatientId == id && !p.IsDeleted);
    }

    public async Task<bool> DeleteAsync(int id, string deletedBy = "System")
    {
        var patient = await _context.Patients.FindAsync(id);
        if (patient == null || patient.IsDeleted)
            return false;

        // Soft delete
        patient.IsDeleted = true;
        patient.DeletedAt = DateTime.UtcNow;
        patient.DeletedBy = deletedBy;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RestoreAsync(int id, string restoredBy = "System")
    {
        var patient = await _context.Patients.FindAsync(id);
        if (patient == null || !patient.IsDeleted)
            return false;

        // Restore from soft delete
        patient.IsDeleted = false;
        patient.DeletedAt = null;
        patient.DeletedBy = null;
        patient.UpdatedAt = DateTime.UtcNow;
        patient.RegisteredBy = restoredBy; // Update who restored it

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AnimalNameExistsForCustomerAsync(string animalName, int customerId, int? excludePatientId = null)
    {
        var query = _context.Patients.Where(p =>
            p.AnimalName == animalName &&
            p.CustomerId == customerId &&
            !p.IsDeleted);

        if (excludePatientId.HasValue)
            query = query.Where(p => p.PatientId != excludePatientId.Value);

        return await query.AnyAsync();
    }

    public async Task<bool> UpdatePhotoAsync(int patientId, string photoUrl)
    {
        var patient = await _context.Patients.FindAsync(patientId);
        if (patient == null || patient.IsDeleted)
            return false;

        patient.PhotoUrl = photoUrl;
        patient.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<string>> GetSpeciesAsync()
    {
        return await _context.Patients
            .Where(p => !p.IsDeleted)
            .Select(p => p.Species)
            .Distinct()
            .OrderBy(s => s)
            .ToListAsync();
    }

    public async Task<IEnumerable<string>> GetBreedsAsync()
    {
        return await _context.Patients
            .Where(p => !p.IsDeleted && !string.IsNullOrEmpty(p.Breed))
            .Select(p => p.Breed)
            .Distinct()
            .OrderBy(b => b)
            .ToListAsync();
    }

    public async Task<(IEnumerable<Patient> Patients, int TotalCount)> SearchPatientsAsync(PatientSearchParameters parameters)
    {
        var query = _context.Patients
            .Include(p => p.Customer)
            .Where(p => !p.IsDeleted)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(parameters.AnimalName))
        {
            query = query.Where(p => p.AnimalName.ToLower().Contains(parameters.AnimalName.ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(parameters.OwnerName))
        {
            var ownerNameLower = parameters.OwnerName.ToLower();
            query = query.Where(p =>
                (p.Customer!.FirstNames + " " + p.Customer.MaternalLastName + " " + p.Customer.PaternalLastName)
                .ToLower().Contains(ownerNameLower) ||
                (p.Customer!.FirstNames + " " + p.Customer.PaternalLastName)
                .ToLower().Contains(ownerNameLower));
        }

        if (!string.IsNullOrWhiteSpace(parameters.OwnerNationalId))
        {
            query = query.Where(p => p.Customer!.DocumentId.Contains(parameters.OwnerNationalId));
        }

        if (!string.IsNullOrWhiteSpace(parameters.Species))
        {
            query = query.Where(p => p.Species.ToLower() == parameters.Species.ToLower());
        }

        if (!string.IsNullOrWhiteSpace(parameters.Breed))
        {
            query = query.Where(p => p.Breed != null && p.Breed.ToLower() == parameters.Breed.ToLower());
        }

        if (parameters.MinAge.HasValue || parameters.MaxAge.HasValue)
        {
            var currentDate = DateTime.Today;

            if (parameters.MinAge.HasValue)
            {
                var maxBirthDate = currentDate.AddYears(-parameters.MinAge.Value);
                query = query.Where(p => p.BirthDate <= maxBirthDate);
            }

            if (parameters.MaxAge.HasValue)
            {
                var minBirthDate = currentDate.AddYears(-parameters.MaxAge.Value - 1);
                query = query.Where(p => p.BirthDate > minBirthDate);
            }
        }

        if (!string.IsNullOrWhiteSpace(parameters.Status))
        {
            if (Enum.TryParse<CustomerStatus>(parameters.Status, true, out var status))
            {
                query = query.Where(p => p.Customer!.CustomerStatus == status);
            }
        }

        var totalCount = await query.CountAsync();

        var patients = await query
            .OrderBy(p => p.AnimalName)
            .ThenBy(p => p.Customer!.FirstNames)
            .Skip((parameters.Page - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        return (patients, totalCount);
    }
}