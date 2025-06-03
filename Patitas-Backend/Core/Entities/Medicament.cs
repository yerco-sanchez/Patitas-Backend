using System.ComponentModel.DataAnnotations;

namespace Patitas_Backend.Core.Entities;

public class Medicament
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string CommercialName { get; set; } = null!;

    [Required]
    [MaxLength(200)]
    public string ActiveIngredient { get; set; } = null!; 

    [Required]
    [MaxLength(100)]
    public string Presentation { get; set; } = null!; 

    [Required]
    [MaxLength(150)]
    public string Laboratory { get; set; } = null!;


    public bool IsDeleted { get; set; } = false;

    public DateTime? DeletedAt { get; set; }

    [MaxLength(100)]
    public string? DeletedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


    public ICollection<MedicamentPrescription>? Prescriptions { get; set; }
}
