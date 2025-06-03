using Patitas_Backend.Core.DTOs;
using Patitas_Backend.Core.Entities;

namespace Patitas_Backend.Core.Mappers;

public static class TreatamentMapper
{
    public static TratamentDto ToDto(this Treatament t)
    {
        return new TratamentDto
        {
            Id = t.Id,
            PatientId = t.PatientId,
            AttentionOrigenId = t.AttentionOrigenId,
            StartDate = t.StartDate,
            EstimatedEndDate = t.EstimatedEndDate,
            RealEndDate = t.RealEndDate,
            GeneralDescription = t.GeneralDescription,
            TreatmentType = t.TreatmentType.ToString(),
            TreatmentStatus = t.TreatmentStatus.ToString(),
            Objective = t.Objective,
            IsDeleted = t.IsDeleted,
            DeletedAt = t.DeletedAt,
            DeletedBy = t.DeletedBy,
            UpdatedAt = t.UpdatedAt,
            CreatedAt = t.CreatedAt,
            medicamentPrescription = t.Prescriptions != null ? t.Prescriptions.ToDto().ToList() : null,
        };
    }

    public static IEnumerable<TratamentDto> ToDto(this IEnumerable<Treatament> list)
    {
        return list.Select(t => t.ToDto());
    }
}
