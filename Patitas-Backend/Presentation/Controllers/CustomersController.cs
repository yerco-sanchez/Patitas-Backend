using Microsoft.AspNetCore.Mvc;
using Patitas_Backend.Core.Entities;
using Patitas_Backend.Core.Interfaces;
using Patitas_Backend.Core.DTOs;
using Patitas_Backend.Infrastructure.Repositories;
using Patitas_Backend.Core.Mappers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IPatientRepository _patientRepository;

    public CustomersController(ICustomerRepository repository, IPatientRepository patientRepository)
    {
        _customerRepository = repository;
        _patientRepository = patientRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CustomerDto>>> GetAll()
    {
        var customers = await _customerRepository.GetAllAsync();
        var customerDtos = customers.ToDto();
        return Ok(customerDtos);
    }

    [HttpGet("deleted")]
    public async Task<ActionResult<IEnumerable<CustomerDto>>> GetAllDeleted()
    {
        var customers = await _customerRepository.GetAllDeletedAsync();
        var customerDtos = customers.ToDto();
        return Ok(customerDtos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerDto>> GetCustomer(int id)
    {
        var customer = await _customerRepository.GetByIdAsync(id);

        if (customer == null)
            return NotFound($"Customer with ID {id} not found.");

        return Ok(customer.ToDto());
    }

    [HttpGet("{id}/pacientes")]
    public async Task<ActionResult<IEnumerable<Patient>>> GetCustomerPatients(int id, [FromQuery] string? search = null)
    {
        if (!await _customerRepository.ExistsAsync(id))
            return NotFound($"Customer with ID {id} not found.");

        var patients = await _patientRepository.GetPatientsByCustomerIdAsync(id);

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLowerInvariant();
            patients = patients.Where(p =>
                p.AnimalName.ToLowerInvariant().Contains(search) ||
                p.Species.ToLowerInvariant().Contains(search) ||
                (!string.IsNullOrEmpty(p.Breed) && p.Breed.ToLowerInvariant().Contains(search))
            );
        }

        return Ok(patients);
    }

    [HttpPost]
    public async Task<ActionResult<CustomerDto>> CreateCustomer(CustomerDto customerDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var (customer, validationErrors) = customerDto.ToEntity();

        if (validationErrors.Any())
        {
            foreach (var error in validationErrors)
            {
                ModelState.AddModelError("", error);
            }
            return BadRequest(ModelState);
        }

        if (customer == null)
            return BadRequest("Error converting customer data.");

        if (await _customerRepository.EmailExistsAsync(customer.Email))
            return Conflict($"A customer with email {customer.Email} already exists.");

        if (await _customerRepository.NationalIdExistsAsync(customer.DocumentId))
            return Conflict($"A customer with National ID {customer.DocumentId} already exists.");

        customer.CustomerId = 0;

        var createdCustomer = await _customerRepository.CreateAsync(customer);

        return CreatedAtAction(
            nameof(GetCustomer),
            new { id = createdCustomer.CustomerId },
            createdCustomer.ToDto()
        );
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCustomer(int id, CustomerDto customerDto)
    {
        if (id != customerDto.CustomerId)
            return BadRequest("ID in URL does not match customer ID.");

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (!await _customerRepository.ExistsAsync(id))
            return NotFound($"Customer with ID {id} not found.");

        var (customer, validationErrors) = customerDto.ToEntity();

        if (validationErrors.Any())
        {
            foreach (var error in validationErrors)
            {
                ModelState.AddModelError("", error);
            }
            return BadRequest(ModelState);
        }

        if (customer == null)
            return BadRequest("Error converting customer data.");

        if (await _customerRepository.EmailExistsAsync(customer.Email, id))
            return Conflict($"A customer with email {customer.Email} already exists.");

        if (await _customerRepository.NationalIdExistsAsync(customer.DocumentId, id))
            return Conflict($"A customer with National ID {customer.DocumentId} already exists.");

        try
        {
            await _customerRepository.UpdateAsync(customer);
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error updating customer: {ex.Message}");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCustomer(int id)
    {
        if (!await _customerRepository.ExistsAsync(id))
            return NotFound($"Customer with ID {id} not found.");

        try
        {
            var currentUser = GetCurrentUser();

            var deleted = await _customerRepository.DeleteAsync(id, currentUser);
            if (deleted)
                return NoContent();
            else
                return StatusCode(500, "Error deleting customer.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error deleting customer: {ex.Message}");
        }
    }

    [HttpPatch("{id}/restore")]
    public async Task<IActionResult> RestoreCustomer(int id)
    {
        if (!await _customerRepository.ExistsDeletedAsync(id))
            return NotFound($"Deleted customer with ID {id} not found.");

        try
        {
            var restored = await _customerRepository.RestoreAsync(id);
            if (restored)
                return NoContent();
            else
                return StatusCode(500, "Error restoring customer.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error restoring customer: {ex.Message}");
        }
    }

    private string? GetCurrentUser()
    {
        return "system";
    }
}