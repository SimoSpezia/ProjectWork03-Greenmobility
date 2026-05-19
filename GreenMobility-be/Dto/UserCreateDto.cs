using GreenMobility_be.Data;
using System.ComponentModel.DataAnnotations;

namespace GreenMobility_be.Dto
{
    public class UserCreateDto
    {
        [MaxLength(100)]
        [Required]
        public required string Name { get; set; }
        [MaxLength(100)]
        [Required]
        public required string Surname { get; set; }
        [EmailAddress]
        [Required]
        public required string Email { get; set; }
        [Required]
        public required string Password { get; set; }
        [Required]
        public required string Role { get; set; }
    }
}
