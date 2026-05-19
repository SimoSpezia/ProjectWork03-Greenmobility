using System.ComponentModel.DataAnnotations;

namespace GreenMobility_be.Data
{
    public class VehicleStatus
    {
        public int VehicleStatusId { get; set; }
        [MaxLength(100)]
        public required string Status { get; set; }
        public List<Vehicle>? Vehicles { get; set; }
    }
}
