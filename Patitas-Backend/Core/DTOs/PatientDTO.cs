using Patitas_Backend.Core.Entities;
using Patitas_Backend.Core.Enumerables;
using System.ComponentModel.DataAnnotations;

namespace Patitas_Backend.Core.DTOs;

public class PatientDto
{
    public int PatientId { get; set; }
    public string AnimalName { get; set; } = null!;
    public string Species { get; set; } = null!;
    public string Breed { get; set; } = null!;
    public string Gender { get; set; } = null!; 
    public DateTime BirthDate { get; set; }
    public int Age { get; set; }
    public decimal Weight { get; set; }
    public string Classification { get; set; } = null!; 
    public string PhotoUrl { get; set; } = "";

    public DateTime RegisteredAt { get; set; }
    public string RegisteredBy { get; set; } = null!;
    public int CustomerId { get; set; }

    public CustomerDto? Customer { get; set; }

    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Patient ToEntity()
    {
        if (!Enum.TryParse<Gender>(Gender, true, out var genderEnum))
            throw new ArgumentException($"Invalid Gender value: {Gender}");

        if (!Enum.TryParse<Classification>(Classification, true, out var classificationEnum))
            throw new ArgumentException($"Invalid Classification value: {Classification}");

        return new Patient
        {
            PatientId = PatientId,
            AnimalName = AnimalName,
            Species = Species,
            Breed = Breed,
            Gender = genderEnum,
            BirthDate = BirthDate,
            Weight = Weight,
            Classification = classificationEnum,
            PhotoUrl = PhotoUrl,
            RegisteredAt = RegisteredAt,
            RegisteredBy = RegisteredBy,
            CustomerId = CustomerId,
            IsDeleted = IsDeleted,
            DeletedAt = DeletedAt,
            DeletedBy = DeletedBy,
            UpdatedAt = UpdatedAt
        };
    }
}
