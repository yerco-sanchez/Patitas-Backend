using Microsoft.AspNetCore.Mvc;
using Patitas_Backend.Core.Interfaces;

namespace Patitas_Backend.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CatalogsController : ControllerBase
{
    private readonly IPatientRepository _patientRepository;

    public CatalogsController(IPatientRepository patientRepository)
    {
        _patientRepository = patientRepository;
    }

    [HttpGet("especies")]
    public async Task<ActionResult<IEnumerable<string>>> GetSpecies()
    {
        try
        {
            var species = await _patientRepository.GetSpeciesAsync();
            return Ok(species);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error retrieving species: {ex.Message}");
        }
    }

    [HttpGet("razas")]
    public async Task<ActionResult<IEnumerable<string>>> GetBreeds()
    {
        try
        {
            var breeds = await _patientRepository.GetBreedsAsync();
            return Ok(breeds);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error retrieving breeds: {ex.Message}");
        }
    }
}