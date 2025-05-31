using Microsoft.AspNetCore.Mvc;
using Patitas_Backend.Core.Entities;
using Patitas_Backend.Core.Interfaces;
using Patitas_Backend.Infrastructure.Repositories;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerRepository _repository;

    public CustomersController(ICustomerRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Customer>>> GetAll()
    {
        var customers = await _repository.GetAllAsync();
        return Ok(customers);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Customer>> GetCustomer(int id)
    {
        var customer = await _repository.GetByIdAsync(id);

        if (customer == null)
            return NotFound($"Customer with ID {id} not found.");

        return Ok(customer);
    }

    [HttpPost]
    public async Task<ActionResult<Customer>> CreateCustomer(Customer customer)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (await _repository.EmailExistsAsync(customer.Email))
            return Conflict($"A customer with email {customer.Email} already exists.");

        if (await _repository.NationalIdExistsAsync(customer.NationalId))
            return Conflict($"A customer with National ID {customer.NationalId} already exists.");

        customer.CustomerId = 0;

        var createdCustomer = await _repository.CreateAsync(customer);

        return CreatedAtAction(nameof(GetCustomer), new { id = createdCustomer.CustomerId }, createdCustomer);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCustomer(int id, Customer customer)
    {
        if (id != customer.CustomerId)
            return BadRequest("ID in URL does not match customer ID.");

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var existCustomer = await _repository.ExistsAsync(id);

        if (!existCustomer)
            return NotFound($"Customer with ID {id} not found.");

        if (await _repository.EmailExistsAsync(customer.Email, id))
            return Conflict($"A customer with email {customer.Email} already exists.");

        if (await _repository.NationalIdExistsAsync(customer.NationalId, id))
            return Conflict($"A customer with National ID {customer.NationalId} already exists.");

        try
        {
            await _repository.UpdateAsync(customer);
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error updating customer: {ex.Message}");
        }
    }

}