using GreenMobility_be.Data;
using GreenMobility_be.Dto;

namespace GreenMobility_be.Mapper
{
    public class VehicleMapper
    {
        public VehicleDto MapEntityToDto(Vehicle entity)
        {
            VehicleDto dto = new VehicleDto()
            {
                VehicleId = entity.VehicleId,
                VehicleTypeId = entity.VehicleTypeId,
                VehicleTypeName = entity.VehicleType?.Type,
                VehicleStatusId = entity.VehicleStatusId,
                VehicleStatusName = entity.VehicleStatus?.Status,
                HubId = entity.HubId,
                HubName = entity.Hub?.Name,
                UIC = entity.UIC,
                BatteryLevel = entity.BatteryLevel,
                IsDeleted = entity.IsDeleted
            };

            return dto;
        }
    }
}
