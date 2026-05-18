namespace GreenMobility_be.Controllers.Data
{
    public class Hub
    {
        public required int HubId { get; set; }
        public required string Name { get; set; }
        public required string City { get; set; }
        public required int MaximumCapacity { get; set; } 

        public List<Vehicle>? Vehicles { get; set; }
    }
}
