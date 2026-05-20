using GreenMobility_be.Data;
using GreenMobility_be.Dto;

namespace GreenMobility_be.Mapper
{
    using System.Linq;

    public class HubMapper
    {
        public HubDto MapEntityToDto(Hub hub)
        {
            if (hub == null) return null!;
            return new HubDto
            {
                Id = hub.HubId,
                Name = hub.Name,
                Address = hub.Address,
                City = hub.City,
                MaximumCapacity = hub.MaximumCapacity,
                Vehicles = hub.Vehicles?.Select(v => new VehicleDto
                {
                    VehicleId = v.VehicleId,
                    UIC = v.UIC,
                    BatteryLevel = v.BatteryLevel,
                    VehicleTypeId = v.VehicleTypeId,
                    VehicleStatusId = v.VehicleStatusId,
                    HubId = v.HubId
                }).ToList()
            };
        }

        public VehicleDto MapEntityToDto(Vehicle v)
        {
            if (v == null) return null!;
            return new VehicleDto
            {
                VehicleId = v.VehicleId,
                UIC = v.UIC,
                BatteryLevel = v.BatteryLevel,
                VehicleTypeId = v.VehicleTypeId,
                VehicleStatusId = v.VehicleStatusId
            };
        }

        public Hub MapDtoToEntity(HubCreateDto dto)
        {
            if (dto == null) return null!;
            return new Hub
            {
                Name = dto.Name,
                Address = dto.Address,
                City = dto.City,
                MaximumCapacity = dto.MaximumCapacity
            };
        }

        public Hub MapDtoToEntity(HubDto dto)
        {
            if (dto == null) return null!;
            return new Hub
            {
                HubId = dto.Id,
                Name = dto.Name,
                Address = dto.Address,
                City = dto.City,
                MaximumCapacity = dto.MaximumCapacity,
                Vehicles = null,
            };
        }
    }
}

