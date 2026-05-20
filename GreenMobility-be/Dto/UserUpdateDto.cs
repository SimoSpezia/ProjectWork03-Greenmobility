using System.ComponentModel.DataAnnotations;

namespace GreenMobility_be.Dto
{
    public class UserUpdateDto
    {
        [MaxLength(100)]
        public string? Name { get; set; }
        [MaxLength(100)]
        public string? Surname { get; set; }
        [EmailAddress]
        public string? Email { get; set; }

    }
}
