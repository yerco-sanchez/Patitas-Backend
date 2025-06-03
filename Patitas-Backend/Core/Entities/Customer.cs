using Microsoft.EntityFrameworkCore;
using Patitas_Backend.Core.Enumerables;
using System.ComponentModel.DataAnnotations;

namespace Patitas_Backend.Core.Entities;

[Index(nameof(DocumentId), IsUnique = true)]
[Index(nameof(Email), IsUnique = true)]
public class Customer
{
    [Key]
    public int CustomerId { get; set; }

    [Required, MaxLength(100)]
    public string FirstNames { get; set; } = null!;

    [Required, MaxLength(50)]
    public string PaternalLastName { get; set; } = null!;

    [Required, MaxLength(50)]
    public string MaternalLastName { get; set; } = null!;

    [Required, MaxLength(20)]
    public string DocumentId { get; set; } = null!;

    [MaxLength(20)]
    public string Phone { get; set; } = "";

    [MaxLength(100), EmailAddress]
    public string Email { get; set; } = "";

    [MaxLength(200)]
    public string Address { get; set; } = "";

    [Required]
    public CustomerType CustomerType { get; set; }

    public string Notes { get; set; } = "";

    [Required]
    public CustomerStatus CustomerStatus { get; set; }

    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<Patient>? Patients { get; set; }
}