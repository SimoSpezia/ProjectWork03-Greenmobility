namespace GreenMobility_be.Controllers.Data
{
    public class RentalCode
    {
        public required int RentalCodeId { get; set; }
        public required string Code { get; set; }
        public required int RentalId { get; set; }
        public Rental? Rental { get; set; }
    }
}

