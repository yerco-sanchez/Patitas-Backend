using Patitas_Backend.Core.DTOs;
using Patitas_Backend.Core.Entities;

namespace Patitas_Backend.Core.Mappers;

public static class CustomerMapper
{
    public static CustomerDto ToDto(this Customer customer)
    {

        return new CustomerDto
        {
            CustomerId = customer.CustomerId,
            FirstNames = customer.FirstNames,
            PaternalLastName = customer.PaternalLastName,
            MaternalLastName = customer.MaternalLastName,
            DocumentId = customer.DocumentId,
            Phone = customer.Phone,
            Email = customer.Email,
            Address = customer.Address,
            CustomerType = customer.CustomerType.ToString(),
            Notes = customer.Notes,
            CustomerStatus = customer.CustomerStatus.ToString(),
            Patients = customer.Patients != null ? customer.Patients.ToDto().ToList() : null
        };
    }

    public static IEnumerable<CustomerDto> ToDto(this IEnumerable<Customer> customers)
    {
        return customers.Select(c => c.ToDto());
    }
}
