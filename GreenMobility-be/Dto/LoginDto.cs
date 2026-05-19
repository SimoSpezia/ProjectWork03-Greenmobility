using System.ComponentModel.DataAnnotations;

namespace GreenMobility_be.Dto
{
    public class LoginDto
    {
        [EmailAddress]
        public required string Email { get; set; }
        [Length(8, 24)]
        public required string Password { get; set; }

    }
}
