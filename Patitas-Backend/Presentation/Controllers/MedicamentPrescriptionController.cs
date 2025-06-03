using Microsoft.AspNetCore.Mvc;
using Patitas_Backend.Core.DTOs;
using Patitas_Backend.Core.Entities;
using Patitas_Backend.Core.Interfaces;
using Patitas_Backend.Core.Mappers;

namespace Patitas_Backend.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MedicamentPrescriptionsController : ControllerBase
{
    private readonly IMedicamentPrescriptionRepository _prescRepo;

    public MedicamentPrescriptionsController(IMedicamentPrescriptionRepository prescRepo)
    {
        _prescRepo = prescRepo;
    }

    /// <summary>
    /// GET /api/medicamentprescriptions?state=active|inactive|all
    /// Si no se pasa state, devuelve solo los activos.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MedicamentPrescriptionDto>>> GetAll([FromQuery] string? state)
    {
        IEnumerable<MedicamentPrescription> entities;

        if (!string.IsNullOrWhiteSpace(state))
        {
            switch (state.ToLower())
            {
                case "inactive":
                    entities = await _prescRepo.GetAllDeletedAsync();
                    break;
                case "all":
                    entities = await _prescRepo.GetAllAsync(includeInactive: true);
                    break;
                case "active":
                default:
                    entities = await _prescRepo.GetAllAsync(includeInactive: false);
                    break;
            }
        }
        else
        {
            entities = await _prescRepo.GetAllAsync(includeInactive: false);
        }

        return Ok(entities.ToDto());
    }

    /// <summary>
    /// GET /api/medicamentprescriptions/{id}
    /// Devuelve solo si existe y no está eliminado.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<MedicamentPrescriptionDto>> GetById(int id)
    {
        var entity = await _prescRepo.GetByIdAsync(id);
        if (entity == null)
            return NotFound();

        return Ok(entity.ToDto());
    }

    /// <summary>
    /// POST /api/medicamentprescriptions
    /// Crea nueva prescripción. </summary>
    [HttpPost]
    public async Task<ActionResult<MedicamentPrescriptionDto>> Create([FromBody] MedicamentPrescriptionDto dto)
    {
        var (entity, errors) = dto.ToEntity();
        if (errors.Any())
            return BadRequest(new { Errors = errors });

        var created = await _prescRepo.CreateAsync(entity!);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created.ToDto());
    }

    /// <summary>
    /// PUT /api/medicamentprescriptions/{id}
    /// Actualiza una prescripción existente. </summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] MedicamentPrescriptionDto dto)
    {
        if (id != dto.Id)
            return BadRequest("El ID de la ruta no coincide con el ID del cuerpo.");

        var existing = await _prescRepo.GetByIdAsync(id);
        if (existing == null)
            return NotFound();

        var (entity, errors) = dto.ToEntity();
        if (errors.Any())
            return BadRequest(new { Errors = errors });

        // Sobrescribir campos editables:
        existing.TreatamentId = entity!.TreatamentId;
        existing.MedicamentId = entity.MedicamentId;
        existing.Dosage = entity.Dosage;
        existing.DosageUnit = entity.DosageUnit;
        existing.Frequency = entity.Frequency;
        existing.AdministrationRoute = entity.AdministrationRoute;
        existing.StartDate = entity.StartDate;
        existing.EndDate = entity.EndDate;
        existing.PrescriptionStatus = entity.PrescriptionStatus;
        // No tocamos CreatedAt ni IsDeleted, DeletedAt, DeletedBy

        await _prescRepo.UpdateAsync(existing);
        return NoContent();
    }

    /// <summary>
    /// PATCH /api/medicamentprescriptions/{id}/inactivate
    /// Borrado lógico de la prescripción. </summary>
    [HttpPatch("{id:int}/inactivate")]
    public async Task<IActionResult> Inactivate(int id)
    {
        var existing = await _prescRepo.GetByIdAsync(id);
        if (existing == null)
            return NotFound();

        var success = await _prescRepo.DeleteAsync(id, User?.Identity?.Name);
        if (!success)
            return BadRequest("No se pudo inactivar la prescripción.");

        return NoContent();
    }

    /// <summary>
    /// PATCH /api/medicamentprescriptions/{id}/restore
    /// Restaura una prescripción previamente inactivada. </summary>
    [HttpPatch("{id:int}/restore")]
    public async Task<IActionResult> Restore(int id)
    {
        var existsDeleted = await _prescRepo.ExistsDeletedAsync(id);
        if (!existsDeleted)
            return NotFound();

        var success = await _prescRepo.RestoreAsync(id);
        if (!success)
            return BadRequest("No se pudo restaurar la prescripción.");

        return NoContent();
    }
}