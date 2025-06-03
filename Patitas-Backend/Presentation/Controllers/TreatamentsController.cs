using Microsoft.AspNetCore.Mvc;
using Patitas_Backend.Core.DTOs;
using Patitas_Backend.Core.Entities;
using Patitas_Backend.Core.Interfaces;
using Patitas_Backend.Core.Mappers;

namespace Patitas_Backend.Presentation.Controllers;
[ApiController]
[Route("api/[controller]")]
public class TreatamentsController : ControllerBase
{
    private readonly ITreatamentRepository _treatRepo;

    public TreatamentsController(ITreatamentRepository treatRepo)
    {
        _treatRepo = treatRepo;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TratamentDto>>> GetAll([FromQuery] string? state)
    {
        IEnumerable<Treatament> entities;

        if (!string.IsNullOrWhiteSpace(state))
        {
            switch (state.ToLower())
            {
                case "inactive":
                    entities = await _treatRepo.GetAllDeletedAsync();
                    break;
                case "all":
                    entities = await _treatRepo.GetAllAsync(includeInactive: true);
                    break;
                case "active":
                default:
                    entities = await _treatRepo.GetAllAsync(includeInactive: false);
                    break;
            }
        }
        else
        {
            entities = await _treatRepo.GetAllAsync(includeInactive: false);
        }

        return Ok(entities.ToDto());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TratamentDto>> GetById(int id)
    {
        var entity = await _treatRepo.GetByIdAsync(id);
        if (entity == null)
            return NotFound();

        return Ok(entity.ToDto());
    }

    [HttpPost]
    public async Task<ActionResult<TratamentDto>> Create([FromBody] TratamentDto dto)
    {
        var (entity, errors) = dto.ToEntity();
        if (errors.Any())
            return BadRequest(new { Errors = errors });

        var created = await _treatRepo.CreateAsync(entity!);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created.ToDto());
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] TratamentDto dto)
    {
        if (id != dto.Id)
            return BadRequest("El ID de la ruta no coincide con el ID del cuerpo.");

        var existing = await _treatRepo.GetByIdAsync(id);
        if (existing == null)
            return NotFound();

        var (entity, errors) = dto.ToEntity();
        if (errors.Any())
            return BadRequest(new { Errors = errors });

        existing.PatientId = entity!.PatientId;
        existing.AttentionOrigenId = entity.AttentionOrigenId;
        existing.StartDate = entity.StartDate;
        existing.EstimatedEndDate = entity.EstimatedEndDate;
        existing.RealEndDate = entity.RealEndDate;
        existing.GeneralDescription = entity.GeneralDescription;
        existing.TreatmentType = entity.TreatmentType;
        existing.TreatmentStatus = entity.TreatmentStatus;
        existing.Objective = entity.Objective;

        await _treatRepo.UpdateAsync(existing);
        return NoContent();
    }

    [HttpPatch("{id:int}/inactivate")]
    public async Task<IActionResult> Inactivate(int id)
    {
        var existing = await _treatRepo.GetByIdAsync(id);
        if (existing == null)
            return NotFound();

        var success = await _treatRepo.DeleteAsync(id, User?.Identity?.Name);
        if (!success)
            return BadRequest("No se pudo inactivar el tratamiento.");

        return NoContent();
    }

    [HttpPatch("{id:int}/restore")]
    public async Task<IActionResult> Restore(int id)
    {
        var existsDeleted = await _treatRepo.ExistsDeletedAsync(id);
        if (!existsDeleted)
            return NotFound();

        var success = await _treatRepo.RestoreAsync(id);
        if (!success)
            return BadRequest("No se pudo restaurar el tratamiento.");

        return NoContent();
    }
}