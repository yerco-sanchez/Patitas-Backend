using Microsoft.EntityFrameworkCore;
using Patitas_Backend.Core.Enumerables;
using System.ComponentModel.DataAnnotations;

namespace Patitas_Backend.Core.Entities;

[Index(nameof(NationalId), IsUnique = true)]
[Index(nameof(Email), IsUnique = true)]
public class Customer
{
        [Key]
        public int CustomerId { get; set; }

        [Required, MaxLength(100)]
        public string FirstNames { get; set; }

        [Required, MaxLength(50)]
        public string LastName { get; set; }

        [Required, MaxLength(50)]
        public string MiddleName { get; set; }

        [Required, MaxLength(20)]
        public string NationalId { get; set; }

        [MaxLength(20)]
        public string Phone { get; set; }

        [Required, MaxLength(100), EmailAddress]
        public string Email { get; set; }

        [MaxLength(200)]
        public string Address { get; set; }

        [Required]
        public CustomerType CustomerType { get; set; }

        public string Notes { get; set; }

        [Required]
        public CustomerStatus CustomerStatus { get; set; }

        public ICollection<Patient> Patients { get; set; }
    }