using GreenMobility_be.Data;
using System.ComponentModel.DataAnnotations;

namespace GreenMobility_be.Dto
{
    public class VehicleDto
    {
        public int VehicleId { get; set; }
        public int VehicleTypeId { get; set; }
        public string? VehicleTypeName { get; set; }
        public int VehicleStatusId { get; set; }
        public string? VehicleStatusName { get; set; }
        public int HubId { get; set; }
        public string? HubName { get; set; }
        [MaxLength(50)]
        public required string UIC { get; set; }
        public int BatteryLevel { get; set; }
        public bool IsDeleted { get; set; }
    }
}
