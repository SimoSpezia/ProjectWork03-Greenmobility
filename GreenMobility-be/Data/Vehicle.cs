using System.ComponentModel.DataAnnotations;

namespace GreenMobility_be.Data
{
    public class Vehicle
    {
        public int VehicleId { get; set; }
        public int VehicleTypeId { get; set; }
        public int VehicleStatusId { get; set; }
        public int HubId { get; set; }
        [MaxLength(50)]
        public required string UIC { get; set; }
        public int BatteryLevel { get; set; }
        public required Guid ApiKey { get; set; }
        public VehicleStatus? VehicleStatus { get; set; }
        public VehicleType? VehicleType { get; set; }
        public Hub? Hub { get; set; }
        public List<Rental>? Rentals { get; set; }

    }
}
