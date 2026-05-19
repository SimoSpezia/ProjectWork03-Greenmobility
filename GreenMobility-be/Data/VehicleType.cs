using System.ComponentModel.DataAnnotations;

namespace GreenMobility_be.Data
{
    public class VehicleType
    {
        public int VehicleTypeId { get; set; }
        [MaxLength(100)]
        public required string Type { get; set; }
        public List<Vehicle>? Vehicles { get; set; }
    }
}
