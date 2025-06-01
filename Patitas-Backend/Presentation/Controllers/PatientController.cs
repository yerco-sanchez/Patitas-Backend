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

    public PatientsController(
        IPatientRepository patientRepository,
        ICustomerRepository customerRepository)
    {
        _patientRepository = patientRepository;
        _customerRepository = customerRepository;
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

}
