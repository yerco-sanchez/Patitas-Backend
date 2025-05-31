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

        return Ok(createdCustomer);
    }
}