using Patitas_Backend.Core.DTOs;
using Patitas_Backend.Core.Entities;

namespace Patitas_Backend.Core.Mappers;

public static class MedicamentPrescriptionMapper
{
    public static MedicamentPrescriptionDto ToDto(this MedicamentPrescription mp)
    {
        return new MedicamentPrescriptionDto
        {
            Id = mp.Id,
            TreatamentId = mp.TreatamentId,
            MedicamentId = mp.MedicamentId,
            Dosage = mp.Dosage,
            DosageUnit = mp.DosageUnit,
            Frequency = mp.Frequency,
            AdministrationRoute = mp.AdministrationRoute,
            StartDate = mp.StartDate,
            EndDate = mp.EndDate,
            PrescriptionStatus = mp.PrescriptionStatus.ToString(),
            IsDeleted = mp.IsDeleted,
            DeletedAt = mp.DeletedAt,
            DeletedBy = mp.DeletedBy,
            UpdatedAt = mp.UpdatedAt,
            CreatedAt = mp.CreatedAt,
            Medicament = mp.Medicament
        };
    }

    public static IEnumerable<MedicamentPrescriptionDto> ToDto(this IEnumerable<MedicamentPrescription> list)
    {
        return list.Select(mp => mp.ToDto());
    }
}
