using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Patitas_Backend.Core.Entities;
using Patitas_Backend.Core.Enumerables;
using Patitas_Backend.Core.Interfaces;
using Patitas_Backend.Infrastructure.Repositories;

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

    [HttpPost]
    public async Task<ActionResult<Patient>> CreatePatient(Patient patient)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (!await _customerRepository.ExistsAsync(patient.CustomerId))
            return BadRequest($"Customer with ID {patient.CustomerId} does not exist.");

        if (await _patientRepository.AnimalNameExistsForCustomerAsync(patient.AnimalName, patient.CustomerId))
            return Conflict($"A patient with name '{patient.AnimalName}' already exists for this customer.");

        try
        {
            patient.PatientId = 0;
            patient.RegisteredBy = "System";

            var createdPatient = await _patientRepository.CreateAsync(patient);
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error creating patient: {ex.Message}");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePatient(int id, Patient patient)
    {
        if (id != patient.PatientId)
            return BadRequest("ID in URL does not match patient ID.");

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (!await _patientRepository.ExistsAsync(id))
            return NotFound($"Patient with ID {id} not found.");

        if (!await _customerRepository.ExistsAsync(patient.CustomerId))
            return BadRequest($"Customer with ID {patient.CustomerId} does not exist.");

        if (await _patientRepository.AnimalNameExistsForCustomerAsync(patient.AnimalName, patient.CustomerId, id))
            return Conflict($"A patient with name '{patient.AnimalName}' already exists for this customer.");

        try
        {
            await _patientRepository.UpdateAsync(patient);
            return NoContent();
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
            var deleted = await _patientRepository.DeleteAsync(id);
            if (!deleted)
                return StatusCode(500, "Error deleting patient.");

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error deleting patient: {ex.Message}");
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
                Data = patients.Select(p => new
                {
                    p.PatientId,
                    p.AnimalName,
                    p.Species,
                    p.Breed,
                    p.Gender,
                    p.Age,
                    p.Weight,
                    p.Classification,
                    p.PhotoUrl,
                    p.RegisteredAt,
                    Owner = new
                    {
                        p.Customer!.CustomerId,
                        p.Customer.FirstNames,
                        p.Customer.MiddleName,
                        p.Customer.LastName,
                        FullName = $"{p.Customer.FirstNames} {p.Customer.MiddleName} {p.Customer.LastName}".Trim(),
                        p.Customer.NationalId,
                        p.Customer.Phone,
                        p.Customer.Email,
                        p.Customer.CustomerStatus
                    }
                }),
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

}
