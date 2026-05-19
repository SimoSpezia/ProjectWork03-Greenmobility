using System.ComponentModel.DataAnnotations;

namespace GreenMobility_be.Dto
{
    public class VehicleCreateDto
    {
        public int VehicleTypeId { get; set; }
        public int HubId { get; set; }
    }
}
