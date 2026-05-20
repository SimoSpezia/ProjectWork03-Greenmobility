namespace GreenMobility_be.Dto
{
    public class UserDto
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public required string Surname { get; set; }
        public required string Email { get; set; }
        public required string Role { get; set; }
        public bool IsDeleted { get; set; }
    }
}
