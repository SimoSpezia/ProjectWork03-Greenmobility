using System.ComponentModel.DataAnnotations;

namespace GreenMobility_be.Dto
{
    public class RegisterDto
    {
        [Length(2, 24)]
        public required string Name { get; set; }
        [Length(2, 24)]
        public required string Surname { get; set; }
        [EmailAddress]
        public required string Email { get; set; }
        [Length(8, 24)]
        [Compare(nameof(ConfirmPassword))]
        public required string Password { get; set; }
        [Length(8, 24)]
        public required string ConfirmPassword { get; set; }
    }
}
