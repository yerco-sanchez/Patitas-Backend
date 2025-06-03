using Microsoft.EntityFrameworkCore;
using Patitas_Backend.Core.Enumerables;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Patitas_Backend.Core.Entities;
[Index(nameof(PatientId))]
[Index(nameof(AttentionOrigenId))]
public class Treatament
{
    [Key]
    public int Id { get; set; } 

    [Required]
    public int PatientId { get; set; }

    public Patient? Paciente { get; set; }


    [Required]
    public int AttentionOrigenId { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; }  

    [DataType(DataType.Date)]
    public DateTime? EstimatedEndDate { get; set; }  

    [DataType(DataType.Date)]
    public DateTime? RealEndDate { get; set; }  


    [Required]
    public string GeneralDescription { get; set; } = null!;  


    [Required]
    public TreatmentType TreatmentType { get; set; } 

    [Required]
    public TreatmentStatus TreatmentStatus { get; set; } 

    [Required]
    public string Objective { get; set; } = null!;  


    public bool IsDeleted { get; set; } = false;

    public DateTime? DeletedAt { get; set; }

    [MaxLength(100)]
    public string? DeletedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


    public ICollection<MedicamentPrescription>? Prescriptions { get; set; }

}
