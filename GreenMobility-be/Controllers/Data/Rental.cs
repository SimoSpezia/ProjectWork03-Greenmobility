namespace GreenMobility_be.Controllers.Data
{
    public class Rental
    {
        public required int RentalId { get; set; }
        public required int UserId { get; set; }
        public required int VehicleId { get; set; }
        public required int RentalCodeId { get; set; }
        public required DateTime StartDate { get; set; }
        public required DateTime EndDate { get; set; }
        public required decimal TotalCost { get; set; }
        // Navigation properties are already nullable.
        public User? User { get; set; }
        public Vehicle? Vehicle { get; set; }
        public RentalCode? Code { get; set; }
    }
}
