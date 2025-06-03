using Microsoft.EntityFrameworkCore;
using Patitas_Backend.Core.Enumerables;
using System.ComponentModel.DataAnnotations;

namespace Patitas_Backend.Core.Entities;

[Index(nameof(TreatamentId))]
[Index(nameof(MedicamentId))]
public class MedicamentPrescription
{
    [Key]
    public int Id { get; set; } 

    [Required]
    public int TreatamentId { get; set; }

    public Treatament? Treatament { get; set; }

    [Required]
    public int MedicamentId { get; set; }

    public Medicament? Medicament { get; set; }


    [Required]
    [MaxLength(50)]
    public string Dosage { get; set; } = null!;  

    [Required]
    [MaxLength(20)]
    public string DosageUnit { get; set; } = null!; 

    [Required]
    [MaxLength(50)]
    public string Frequency { get; set; } = null!;  

    [Required]
    [MaxLength(50)]
    public string AdministrationRoute { get; set; } = null!;  

    [Required]
    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; } 

    [DataType(DataType.Date)]
    public DateTime? EndDate { get; set; }  

    [Required]
    public PrescriptionStatus PrescriptionStatus { get; set; } 

    public bool IsDeleted { get; set; } = false;

    public DateTime? DeletedAt { get; set; }

    [MaxLength(100)]
    public string? DeletedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

}