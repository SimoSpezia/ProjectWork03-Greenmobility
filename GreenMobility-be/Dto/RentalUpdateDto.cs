namespace GreenMobility_be.Dto
{
    public class RentalUpdateDto
    {
        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; } 
        public int? TotalCost { get; set; }
        public int? BatteryLevel { get; set; }
    }
}