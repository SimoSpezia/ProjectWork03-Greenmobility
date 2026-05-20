namespace GreenMobility_be.Dto
{
    public class RentalUpdateDto
    {
        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; } 
        public decimal? TotalCost { get; set; }
        public int? BatteryLevel { get; set; }
    }
}