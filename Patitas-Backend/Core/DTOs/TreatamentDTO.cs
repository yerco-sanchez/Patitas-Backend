using Patitas_Backend.Core.Entities;
using Patitas_Backend.Core.Enumerables;
using System.ComponentModel.DataAnnotations;

namespace Patitas_Backend.Core.DTOs;

public class TratamentDto
{
    public int Id { get; set; }

    [Required]
    public int PatientId { get; set; }

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
    public string TreatmentType { get; set; } = null!;   // enum como texto

    [Required]
    public string TreatmentStatus { get; set; } = null!; // enum como texto

    [Required]
    public string Objective { get; set; } = null!;

    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    [MaxLength(100)]
    public string? DeletedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<MedicamentPrescriptionDto>? medicamentPrescription { get; set; }

    public (Treatament? entity, List<string> errors) ToEntity()
    {
        var errors = new List<string>();

        // Parsers para enums
        if (!Enum.TryParse<TreatmentType>(this.TreatmentType, true, out var parsedType))
        {
            errors.Add($"Invalid TreatmentType: '{this.TreatmentType}'. Valores válidos: {string.Join(", ", Enum.GetNames<TreatmentType>())}");
        }

        if (!Enum.TryParse<TreatmentStatus>(this.TreatmentStatus, true, out var parsedStatus))
        {
            errors.Add($"Invalid TreatmentStatus: '{this.TreatmentStatus}'. Valores válidos: {string.Join(", ", Enum.GetNames<TreatmentStatus>())}");
        }

        if (errors.Count > 0)
        {
            return (null, errors);
        }

        var entity = new Treatament
        {
            Id = this.Id,
            PatientId = this.PatientId,
            AttentionOrigenId = this.AttentionOrigenId,
            StartDate = this.StartDate,
            EstimatedEndDate = this.EstimatedEndDate,
            RealEndDate = this.RealEndDate,
            GeneralDescription = this.GeneralDescription,
            TreatmentType = parsedType,
            TreatmentStatus = parsedStatus,
            Objective = this.Objective,
            IsDeleted = this.IsDeleted,
            DeletedAt = this.DeletedAt,
            DeletedBy = this.DeletedBy,
            UpdatedAt = this.UpdatedAt,
            CreatedAt = this.CreatedAt,
        };

        return (entity, errors);
    }
}