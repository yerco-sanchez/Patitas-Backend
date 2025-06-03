using Microsoft.EntityFrameworkCore;
using Patitas_Backend.Core.Entities;

namespace Patitas_Backend.Infrastructure.Data;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options)
        : base(options)
    {
    }

    public DbSet<Customer> Customers { get; set; }
    public DbSet<Patient> Patients { get; set; }
    public DbSet<Medicament> Medicaments{ get; set; }
    public DbSet<Treatament> Treataments { get; set; }
    public DbSet<MedicamentPrescription> MedicamentPrescriptions { get; set; }
}
