using Microsoft.AspNetCore.Mvc;
using Patitas_Backend.Core.Entities;
using Patitas_Backend.Core.Interfaces;

namespace Patitas_Backend.Presentation.Controllers;


[ApiController]
[Route("api/[controller]")]
public class MedicamentController : ControllerBase
{
    private readonly IMedicamentRepository _repository;

    public MedicamentController(IMedicamentRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Medicament>>> GetAll([FromQuery] string? state)
    {
        bool includeInactive = false;

        if (!string.IsNullOrEmpty(state))
        {
            switch (state.ToLower())
            {
                case "inactive":
                    includeInactive = true;
                    var inactivos = await _repository.GetAllDeletedAsync();
                    return Ok(inactivos);

                case "all":
                    includeInactive = true;
                    break;

                case "active":
                default:
                    includeInactive = false;
                    break;
            }
        }

        var lista = await _repository.GetAllAsync(includeInactive);
        return Ok(lista);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Medicament>> GetById(int id)
    {
        var medicamento = await _repository.GetByIdAsync(id);
        if (medicamento == null)
            return NotFound();

        return Ok(medicamento);
    }

    [HttpPost]
    public async Task<ActionResult<Medicament>> Create( Medicament medicamento)
    {
        if (string.IsNullOrWhiteSpace(medicamento.CommercialName)
            || string.IsNullOrWhiteSpace(medicamento.ActiveIngredient)
            || string.IsNullOrWhiteSpace(medicamento.Presentation)
            || string.IsNullOrWhiteSpace(medicamento.Laboratory))
        {
            return BadRequest("Todos los campos son obligatorios.");
        }

        if (await _repository.CommercialNameExistsAsync(medicamento.CommercialName))
        {
            return Conflict($"Ya existe un medicamento con nombre comercial '{medicamento.CommercialName}'.");
        }

        var nuevo = new Medicament
        {
            CommercialName = medicamento.CommercialName,
            ActiveIngredient = medicamento.ActiveIngredient,
            Presentation = medicamento.Presentation,
            Laboratory = medicamento.Laboratory,
        };

        var creado = await _repository.CreateAsync(nuevo);
        return CreatedAtAction(nameof(GetById), new { id = creado.Id }, creado);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<Medicament>> Update(int id, [FromBody] Medicament dto)
    {
        if (id != dto.Id)
            return BadRequest("El ID de la URL no coincide con el ID del cuerpo.");

        var existente = await _repository.GetByIdAsync(id);
        if (existente == null)
            return NotFound($"No se encontró un medicamento activo con ID {id}.");

        if (await _repository.CommercialNameExistsAsync(dto.CommercialName, id))
        {
            return Conflict($"Ya existe otro medicamento con nombre comercial '{dto.CommercialName}'.");
        }

        existente.CommercialName = dto.CommercialName;
        existente.ActiveIngredient = dto.ActiveIngredient;
        existente.Presentation = dto.Presentation;
        existente.Laboratory = dto.Laboratory;

        var actualizado = await _repository.UpdateAsync(existente);
        return Ok(actualizado);
    }

    [HttpPatch("{id:int}/inactivate")]
    public async Task<IActionResult> Inactivate(int id)
    {
        var existente = await _repository.GetByIdAsync(id);
        if (existente == null)
            return NotFound($"No se encontró un medicamento activo con ID {id}.");


        var result = await _repository.DeleteAsync(id, "System");
        if (!result)
            return BadRequest("No se pudo inactivar el medicamento.");

        return NoContent();
    }
}
