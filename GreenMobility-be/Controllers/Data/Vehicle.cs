namespace GreenMobility_be.Controllers.Data
{
    public class Vehicle
    {
        public int VehicleId { get; set; }

        // foreign keys
        public int VehicleTypeId { get; set; }
        public int VehicleStatusId { get; set; }
        public int HubId { get; set; }

        public int BatteryLevel { get; set; }

        // navigation
        public VehicleStatus? Status { get; set; }
        public VehicleType? VehicleType { get; set; }
        public Hub? Hub { get; set; }
        public List<Rental>? Rentals { get; set; }

    }
}
