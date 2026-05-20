using GreenMobility_be.Data;
using System.ComponentModel.DataAnnotations;

namespace GreenMobility_be.Dto
{
    public class VehicleUpdateDto
    {
        public int? VehicleTypeId { get; set; }
        public int? VehicleStatusId { get; set; }
        public int? HubId { get; set; }
        [Range(0, 100)]
        public int? BatteryLevel { get; set; }
    }
}
