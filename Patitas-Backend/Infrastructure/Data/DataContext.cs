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
}
