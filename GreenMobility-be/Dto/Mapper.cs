using GreenMobility_be.Data;

namespace GreenMobility_be.Dto
{
    using System.Linq;

    public static class Mapper
    {
        public static HubDto MapEntityToDto(Hub hub)
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
                    Id = v.VehicleId,
                    UIC = v.UIC,
                    BatteryLevel = v.BatteryLevel,
                    VehicleTypeId = v.VehicleTypeId,
                    VehicleStatusId = v.VehicleStatusId
                }).ToList()
                ,
            };
        }

        public static VehicleDto MapEntityToDto(Vehicle v)
        {
            if (v == null) return null!;
            return new VehicleDto
            {
                Id = v.VehicleId,
                UIC = v.UIC,
                BatteryLevel = v.BatteryLevel,
                VehicleTypeId = v.VehicleTypeId,
                VehicleStatusId = v.VehicleStatusId
            };
        }

        public static Hub MapDtoToEntity(HubCreateDto dto)
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

        public static Hub MapDtoToEntity(HubDto dto)
        {
            if (dto == null) return null!;
            return new Hub
            {
                HubId = dto.Id,
                Name = dto.Name,
                Address = dto.Address,
                City = dto.City,
                MaximumCapacity = dto.MaximumCapacity,
                // Do not create Vehicle entities from DTO here (ApiKey and other required fields missing).
                Vehicles = null,
            };
        }

        public static void MapUpdateEntity(HubUpdateDto dto, Hub hub)
        {
            if (dto == null || hub == null) return;
            if (!string.IsNullOrEmpty(dto.Name)) hub.Name = dto.Name;
            if (!string.IsNullOrEmpty(dto.Address)) hub.Address = dto.Address;
            if (!string.IsNullOrEmpty(dto.City)) hub.City = dto.City;
            if (dto.MaximumCapacity.HasValue) hub.MaximumCapacity = dto.MaximumCapacity.Value;
        }
    }
}

