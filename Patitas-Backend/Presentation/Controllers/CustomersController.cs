using Microsoft.AspNetCore.Mvc;
using Patitas_Backend.Core.Entities;
using Patitas_Backend.Core.Interfaces;
using Patitas_Backend.Infrastructure.Repositories;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerRepository _cusromerRepository;
    private readonly IPatientRepository _patientRepository;

    public CustomersController(ICustomerRepository repository, IPatientRepository patientRepository)
    {
        _cusromerRepository = repository;
        _patientRepository = patientRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Customer>>> GetAll()
    {
        var customers = await _cusromerRepository.GetAllAsync();
        return Ok(customers);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Customer>> GetCustomer(int id)
    {
        var customer = await _cusromerRepository.GetByIdAsync(id);

        if (customer == null)
            return NotFound($"Customer with ID {id} not found.");

        return Ok(customer);
    }

    [HttpGet("{id}/pacientes")]
    public async Task<ActionResult<IEnumerable<Patient>>> GetCustomerPatients(int id, [FromQuery] string? search = null)
    {
        if (!await _cusromerRepository.ExistsAsync(id))
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
    public async Task<ActionResult<Customer>> CreateCustomer(Customer customer)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (await _cusromerRepository.EmailExistsAsync(customer.Email))
            return Conflict($"A customer with email {customer.Email} already exists.");

        if (await _cusromerRepository.NationalIdExistsAsync(customer.NationalId))
            return Conflict($"A customer with National ID {customer.NationalId} already exists.");

        customer.CustomerId = 0;

        var createdCustomer = await _cusromerRepository.CreateAsync(customer);

        return CreatedAtAction(nameof(GetCustomer), new { id = createdCustomer.CustomerId }, createdCustomer);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCustomer(int id, Customer customer)
    {
        if (id != customer.CustomerId)
            return BadRequest("ID in URL does not match customer ID.");

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var existCustomer = await _cusromerRepository.ExistsAsync(id);

        if (!existCustomer)
            return NotFound($"Customer with ID {id} not found.");

        if (await _cusromerRepository.EmailExistsAsync(customer.Email, id))
            return Conflict($"A customer with email {customer.Email} already exists.");

        if (await _cusromerRepository.NationalIdExistsAsync(customer.NationalId, id))
            return Conflict($"A customer with National ID {customer.NationalId} already exists.");

        try
        {
            await _cusromerRepository.UpdateAsync(customer);
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
        var exists = await _cusromerRepository.ExistsAsync(id);
        if (!exists)
            return NotFound($"Customer with ID {id} not found.");

        try
        {
            var deleted = await _cusromerRepository.DeleteAsync(id);
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
}