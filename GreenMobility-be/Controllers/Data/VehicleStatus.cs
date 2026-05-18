namespace GreenMobility_be.Controllers.Data
{
    public class VehicleStatus
    {
        public required int VehicleStatusId { get; set; }
        public required string Status { get; set; }
        public List<Vehicle>? Vehicles { get; set; }
    }
}
