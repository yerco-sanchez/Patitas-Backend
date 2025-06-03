using System.ComponentModel.DataAnnotations;
using Patitas_Backend.Core.Entities;
using Patitas_Backend.Core.Enumerables;

namespace Patitas_Backend.Core.DTOs;

public class CustomerDto
{
    public int CustomerId { get; set; }

    [Required, MaxLength(100)]
    public string FirstNames { get; set; } = null!;

    [Required, MaxLength(50)]
    public string PaternalLastName { get; set; } = null!;

    [MaxLength(50)]
    public string MaternalLastName { get; set; } = "";

    [Required, MaxLength(20)]
    public string DocumentId { get; set; } = null!;

    [MaxLength(20)]
    public string Phone { get; set; } = "";

    [MaxLength(100), EmailAddress]
    public string Email { get; set; } = "";

    [MaxLength(200)]
    public string Address { get; set; } = "";

    public string? CustomerType { get; set; }

    public string Notes { get; set; } = "";

    public string? CustomerStatus { get; set; }

    public string FullName => $"{FirstNames} {PaternalLastName} {MaternalLastName}";

    public DateTime CreatedAt { get; set; }

    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<PatientDto>? Patients { get; set; }

    public (Customer? customer, List<string> errors) ToEntity()
    {
        var errors = new List<string>();

        if (!Enum.TryParse<CustomerType>(CustomerType, true, out var customerType))
        {
            errors.Add($"Invalid CustomerType: '{CustomerType}'. Valid values are: {string.Join(", ", Enum.GetNames<CustomerType>())}");
        }
        if (!Enum.TryParse<CustomerStatus>(CustomerStatus, true, out  var customerStatus))
        {
            errors.Add($"Invalid CustomerStatus: '{CustomerStatus}'. Valid values are: {string.Join(", ", Enum.GetNames<CustomerStatus>())}");
        }

        if (errors.Any())
        {
            return (null, errors);
        }

        var customer = new Customer
        {
            CustomerId = this.CustomerId,
            FirstNames = this.FirstNames,
            PaternalLastName = this.PaternalLastName,
            MaternalLastName = this.MaternalLastName,
            DocumentId = this.DocumentId,
            Phone = this.Phone,
            Email = this.Email,
            Address = this.Address,
            CustomerType = customerType,
            Notes = this.Notes,
            CustomerStatus = customerStatus,
            CreatedAt = this.CreatedAt,
            UpdatedAt = this.UpdatedAt,
            DeletedAt = this.DeletedAt,
            DeletedBy = this.DeletedBy,
            IsDeleted = this.IsDeleted
        };

        return (customer, errors);
    }
}
