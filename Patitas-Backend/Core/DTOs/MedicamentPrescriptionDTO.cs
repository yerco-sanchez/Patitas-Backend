using Patitas_Backend.Core.Entities;
using Patitas_Backend.Core.Enumerables;
using System.ComponentModel.DataAnnotations;

namespace Patitas_Backend.Core.DTOs;

public class MedicamentPrescriptionDto
{
    public int Id { get; set; }

    [Required]
    public int TreatamentId { get; set; }

    [Required]
    public int MedicamentId { get; set; }

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
    public string PrescriptionStatus { get; set; } = null!;   

    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    [MaxLength(100)]
    public string? DeletedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Medicament? Medicament { get; set; }

    public (MedicamentPrescription? entity, List<string> errors) ToEntity()
    {
        var errors = new List<string>();

        if (!Enum.TryParse<PrescriptionStatus>(this.PrescriptionStatus, true, out var parsedStatus))
        {
            errors.Add($"Invalid PrescriptionStatus: '{this.PrescriptionStatus}'. Valores válidos: {string.Join(", ", Enum.GetNames<PrescriptionStatus>())}");
        }

        if (errors.Count > 0)
        {
            return (null, errors);
        }

        var entity = new MedicamentPrescription
        {
            Id = this.Id,
            TreatamentId = this.TreatamentId,
            MedicamentId = this.MedicamentId,
            Dosage = this.Dosage,
            DosageUnit = this.DosageUnit,
            Frequency = this.Frequency,
            AdministrationRoute = this.AdministrationRoute,
            StartDate = this.StartDate,
            EndDate = this.EndDate,
            PrescriptionStatus = parsedStatus,
            IsDeleted = this.IsDeleted,
            DeletedAt = this.DeletedAt,
            DeletedBy = this.DeletedBy,
            UpdatedAt = this.UpdatedAt,
            CreatedAt = this.CreatedAt,
            Medicament = this.Medicament
        };

        return (entity, errors);
    }
}