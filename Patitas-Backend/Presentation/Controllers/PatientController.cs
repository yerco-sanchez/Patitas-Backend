using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Patitas_Backend.Core.Entities;
using Patitas_Backend.Core.Enumerables;
using Patitas_Backend.Core.Interfaces;
using Patitas_Backend.Core.DTOs;
using Patitas_Backend.Infrastructure.Repositories;
using Patitas_Backend.Core.Mappers;

namespace Patitas_Backend.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PatientsController : ControllerBase
{
    private readonly IPatientRepository _patientRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IWebHostEnvironment _environment;

    public PatientsController(
        IPatientRepository patientRepository,
        ICustomerRepository customerRepository,
        IWebHostEnvironment environment)
    {
        _patientRepository = patientRepository;
        _customerRepository = customerRepository;
        _environment = environment;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PatientDto>>> GetAllPatients()
    {
        try
        {
            var patients = await _patientRepository.GetAllPatientsAsync();
            return Ok(patients.ToDto());
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error retrieving patients: {ex.Message}");
        }
    }

    [HttpGet("deleted")]
    public async Task<ActionResult<IEnumerable<PatientDto>>> GetDeletedPatients()
    {
        try
        {
            var patients = await _patientRepository.GetDeletedPatientsAsync();
            return Ok(patients.ToDto());
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error retrieving deleted patients: {ex.Message}");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PatientDto>> GetPatient(int id)
    {
        try
        {
            var patient = await _patientRepository.GetByIdAsync(id);
            if (patient == null)
                return NotFound($"Patient with ID {id} not found.");

            return Ok(patient.ToDto());
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error retrieving patient: {ex.Message}");
        }
    }

    [HttpGet("customer/{customerId}")]
    public async Task<ActionResult<IEnumerable<PatientDto>>> GetPatientsByCustomer(int customerId)
    {
        try
        {
            if (!await _customerRepository.ExistsAsync(customerId))
                return BadRequest($"Customer with ID {customerId} does not exist.");

            var patients = await _patientRepository.GetPatientsByCustomerIdAsync(customerId);
            return Ok(patients.ToDto());
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error retrieving patients: {ex.Message}");
        }
    }

    [HttpPost]
    public async Task<ActionResult<PatientDto>> CreatePatient(PatientDto patientDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (!await _customerRepository.ExistsAsync(patientDto.CustomerId))
            return BadRequest($"Customer with ID {patientDto.CustomerId} does not exist.");

        if (await _patientRepository.AnimalNameExistsForCustomerAsync(patientDto.AnimalName, patientDto.CustomerId))
            return Conflict($"A patient with name '{patientDto.AnimalName}' already exists for this customer.");

        try
        {
            var patient = patientDto.ToEntity();
            patient.PatientId = 0;
            patient.RegisteredBy = "System";

            var createdPatient = await _patientRepository.CreateAsync(patient);
            var createdPatientDto = createdPatient.ToDto();

            return CreatedAtAction(nameof(GetPatient), new { id = createdPatient.PatientId }, createdPatientDto);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error creating patient: {ex.Message}");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePatient(int id, PatientDto patientDto)
    {
        if (id != patientDto.PatientId)
            return BadRequest("ID in URL does not match patient ID.");

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (!await _patientRepository.ExistsAsync(id))
            return NotFound($"Patient with ID {id} not found.");

        if (!await _customerRepository.ExistsAsync(patientDto.CustomerId))
            return BadRequest($"Customer with ID {patientDto.CustomerId} does not exist.");

        if (await _patientRepository.AnimalNameExistsForCustomerAsync(patientDto.AnimalName, patientDto.CustomerId, id))
            return Conflict($"A patient with name '{patientDto.AnimalName}' already exists for this customer.");

        try
        {
            var patient = patientDto.ToEntity();
            await _patientRepository.UpdateAsync(patient);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error updating patient: {ex.Message}");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePatient(int id)
    {
        if (!await _patientRepository.ExistsAsync(id))
            return NotFound($"Patient with ID {id} not found.");

        try
        {
            var deleted = await _patientRepository.DeleteAsync(id, "System");
            if (!deleted)
                return StatusCode(500, "Error deleting patient.");

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error deleting patient: {ex.Message}");
        }
    }

    [HttpPost("{id}/restore")]
    public async Task<IActionResult> RestorePatient(int id)
    {
        try
        {
            var restored = await _patientRepository.RestoreAsync(id, "System");
            if (!restored)
                return NotFound($"Patient with ID {id} not found or not deleted.");

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error restoring patient: {ex.Message}");
        }
    }

    [HttpPost("{id}/foto")]
    public async Task<IActionResult> UploadPhoto(int id, IFormFile photo)
    {
        if (!await _patientRepository.ExistsAsync(id))
            return NotFound($"Patient with ID {id} not found.");

        if (photo == null || photo.Length == 0)
            return BadRequest("No photo file provided.");

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
        var fileExtension = Path.GetExtension(photo.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(fileExtension))
            return BadRequest("Invalid file type. Only JPG, PNG and GIF files are allowed.");

        if (photo.Length > 5 * 1024 * 1024)
            return BadRequest("File size exceeds 5MB limit.");

        try
        {
            var contentRoot = _environment.ContentRootPath;
            var webRootPath = _environment.WebRootPath ?? Path.Combine(contentRoot, "wwwroot");

            var uploadsFolder = Path.Combine(webRootPath, "uploads", "patients");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"patient_{id}_{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await photo.CopyToAsync(stream);
            }

            var photoUrl = $"/uploads/patients/{fileName}";
            var updated = await _patientRepository.UpdatePhotoAsync(id, photoUrl);

            if (!updated)
                return StatusCode(500, "Error updating patient photo URL.");

            return Ok(new { photoUrl });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error uploading photo: {ex.Message}");
        }
    }

    [HttpGet("busqueda-avanzada")]
    public async Task<ActionResult> SearchPatients(
        [FromQuery] string? animalName = null,
        [FromQuery] string? ownerName = null,
        [FromQuery] string? ownerNationalId = null,
        [FromQuery] string? species = null,
        [FromQuery] string? breed = null,
        [FromQuery] int? minAge = null,
        [FromQuery] int? maxAge = null,
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            if (page < 1)
                return BadRequest("Page number must be greater than 0.");

            if (pageSize < 1 || pageSize > 100)
                return BadRequest("Page size must be between 1 and 100.");

            if (minAge.HasValue && minAge < 0)
                return BadRequest("Minimum age cannot be negative.");

            if (maxAge.HasValue && maxAge < 0)
                return BadRequest("Maximum age cannot be negative.");

            if (minAge.HasValue && maxAge.HasValue && minAge > maxAge)
                return BadRequest("Minimum age cannot be greater than maximum age.");

            if (!string.IsNullOrWhiteSpace(status))
            {
                if (!Enum.TryParse<CustomerStatus>(status, true, out _))
                {
                    return BadRequest($"Invalid status. Valid values are: {string.Join(", ", Enum.GetNames<CustomerStatus>())}");
                }
            }

            var searchParameters = new PatientSearchParameters
            {
                AnimalName = animalName,
                OwnerName = ownerName,
                OwnerNationalId = ownerNationalId,
                Species = species,
                Breed = breed,
                MinAge = minAge,
                MaxAge = maxAge,
                Status = status,
                Page = page,
                PageSize = pageSize
            };

            var (patients, totalCount) = await _patientRepository.SearchPatientsAsync(searchParameters);

            var response = new
            {
                Data = patients.ToDto(),
                Pagination = new
                {
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalRecords = totalCount,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                    HasPreviousPage = page > 1,
                    HasNextPage = page < Math.Ceiling((double)totalCount / pageSize)
                },
                SearchCriteria = new
                {
                    AnimalName = animalName,
                    OwnerName = ownerName,
                    OwnerNationalId = ownerNationalId,
                    Species = species,
                    Breed = breed,
                    MinAge = minAge,
                    MaxAge = maxAge,
                    Status = status
                }
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error performing advanced search: {ex.Message}");
        }
    }

    [HttpGet("filtros")]
    public async Task<ActionResult> GetSearchFilters()
    {
        try
        {
            var species = await _patientRepository.GetSpeciesAsync();
            var breeds = await _patientRepository.GetBreedsAsync();
            var statuses = Enum.GetNames<CustomerStatus>().ToList();
            var genders = Enum.GetNames<Gender>().ToList();
            var classifications = Enum.GetNames<Classification>().ToList();

            var filters = new
            {
                Species = species,
                Breeds = breeds,
                Statuses = statuses,
                Genders = genders,
                Classifications = classifications
            };

            return Ok(filters);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error retrieving search filters: {ex.Message}");
        }
    }
}