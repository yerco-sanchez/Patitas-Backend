using Patitas_Backend.Core.DTOs;
using Patitas_Backend.Core.Entities;

namespace Patitas_Backend.Core.Mappers;

public static class PatientMapper
{
    public static PatientDto ToDto(this Patient patient)
    {
        return new PatientDto
        {
            PatientId = patient.PatientId,
            AnimalName = patient.AnimalName,
            Species = patient.Species,
            Breed = patient.Breed,
            Gender = patient.Gender.ToString(),
            BirthDate = patient.BirthDate,
            Age = patient.Age,
            Weight = patient.Weight,
            Classification = patient.Classification.ToString(),
            PhotoUrl = patient.PhotoUrl,
            RegisteredAt = patient.RegisteredAt,
            RegisteredBy = patient.RegisteredBy,
            CustomerId = patient.CustomerId,
            Customer = patient.Customer != null ? new CustomerDto
            {
                CustomerId = patient.Customer.CustomerId,
                FirstNames = patient.Customer.FirstNames,
                MaternalLastName = patient.Customer.MaternalLastName,
                PaternalLastName = patient.Customer.PaternalLastName,
                DocumentId = patient.Customer.DocumentId,
                Phone = patient.Customer.Phone,
                Email = patient.Customer.Email,
                Address = patient.Customer.Address,
                CustomerType = patient.Customer.CustomerType.ToString(),
                CustomerStatus = patient.Customer.CustomerStatus.ToString(),
                Notes = patient.Customer.Notes,
                CreatedAt = patient.Customer.CreatedAt,
                UpdatedAt = patient.Customer.UpdatedAt
            } : null,
            IsDeleted = patient.IsDeleted,
            DeletedAt = patient.DeletedAt,
            DeletedBy = patient.DeletedBy,
            UpdatedAt = patient.UpdatedAt
        };
    }

    public static IEnumerable<PatientDto> ToDto(this IEnumerable<Patient> patients)
    {
        return patients.Select(c => c.ToDto());
    }
}
