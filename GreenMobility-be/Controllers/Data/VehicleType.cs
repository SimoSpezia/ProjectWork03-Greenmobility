namespace GreenMobility_be.Controllers.Data
{
    public class VehicleType
    {
        public required int VehicleTypeId { get; set; }
        public required string Type { get; set; }
        public List<Vehicle>? Vehicles { get; set; }
    }
}
