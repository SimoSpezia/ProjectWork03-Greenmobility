using GreenMobility_be.Data;
using System.ComponentModel.DataAnnotations;

namespace GreenMobility_be.Dto
{
    public class UserCreateDto
    {
        [MaxLength(100)]
        public required string Name { get; set; }
        [MaxLength(100)]
        public required string Surname { get; set; }
        [EmailAddress]
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string Role { get; set; }
    }
}
