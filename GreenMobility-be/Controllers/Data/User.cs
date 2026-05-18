namespace GreenMobility_be.Controllers.Data
{
    public class User
    {
        public int UserId { get; set; }
        public required string Name { get; set; }
        public required string Surname { get; set; } 
        public required string Email { get; set; }
        public required string Role { get; set; }        
        public List<Rental>? Rentals { get; set; }
    }
}
