using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Patitas_Backend.Core.Entities;
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
}
