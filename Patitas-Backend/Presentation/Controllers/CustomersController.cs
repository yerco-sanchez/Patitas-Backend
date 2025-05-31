using Microsoft.AspNetCore.Mvc;
using Patitas_Backend.Core.Entities;
using Patitas_Backend.Core.Interfaces;

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
}