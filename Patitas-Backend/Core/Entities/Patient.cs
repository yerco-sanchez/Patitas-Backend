using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Patitas_Backend.Core.Enumerables;

namespace Patitas_Backend.Core.Entities;


[Index(nameof(AnimalName), nameof(CustomerId), IsUnique = true)]
public class Patient
{
    [Key]
    public int PatientId { get; set; }

    [Required, MaxLength(100)]
    public string AnimalName { get; set; } = null!;

    [Required, MaxLength(50)]
    public string Species { get; set; } = null!;

    [MaxLength(50)]
    public string Breed { get; set; } = null!;

    [Required]
    public Gender Gender { get; set; }

    [Column(TypeName = "date")]
    public DateTime BirthDate { get; set; }

    [NotMapped]
    public int Age =>
        BirthDate == default
            ? 0
            : DateTime.Today.Year - BirthDate.Year - (DateTime.Today.DayOfYear < BirthDate.DayOfYear ? 1 : 0);

    [Column(TypeName = "decimal(5,2)")]
    public decimal Weight { get; set; }

    [Required]
    public Classification Classification { get; set; }

    [MaxLength(200)]
    public string PhotoUrl { get; set; } = null!;

    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

    [MaxLength(50)]
    public string RegisteredBy { get; set; } = null!;

    [Required]
    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }
}