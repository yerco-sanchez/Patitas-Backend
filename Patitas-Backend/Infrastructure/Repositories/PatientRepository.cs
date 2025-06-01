using Microsoft.EntityFrameworkCore;
using Patitas_Backend.Core.Entities;
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
            .Where(p => p.CustomerId == customerId)
            .Include(p => p.Customer)
            .OrderBy(p => p.AnimalName)
            .ToListAsync();
    }

    public async Task<Patient> CreateAsync(Patient patient)
    {
        patient.RegisteredAt = DateTime.UtcNow;
        _context.Patients.Add(patient);
        await _context.SaveChangesAsync();
        return patient;
    }
    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Patients.AnyAsync(p => p.PatientId == id);
    }


    public async Task<bool> AnimalNameExistsForCustomerAsync(string animalName, int customerId, int? excludePatientId = null)
    {
        var query = _context.Patients.Where(p => p.AnimalName == animalName && p.CustomerId == customerId);

        if (excludePatientId.HasValue)
            query = query.Where(p => p.PatientId != excludePatientId.Value);

        return await query.AnyAsync();
    }
    public async Task<bool> UpdatePhotoAsync(int patientId, string photoUrl)
    {
        var patient = await _context.Patients.FindAsync(patientId);
        if (patient == null)
            return false;

        patient.PhotoUrl = photoUrl;
        await _context.SaveChangesAsync();
        return true;
    }
}
