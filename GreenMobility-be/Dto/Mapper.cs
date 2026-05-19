using GreenMobility_be.Data;
using GreenMobility_be.Dto;
using System.Linq;

namespace GreenMobility_be.Data
{
    public static class Mapper
    {
        public static RentalDto MapEntityToDto(Rental r)
        {
            if (r == null) return null!;
            return new RentalDto
            {
                UserId = r.UserId,
                VehicleId = r.VehicleId,
                StartDate = r.StartDate,
                EndDate = r.EndDate,
                TotalCost = r.TotalCost,
                RentalCode = r.RentalCode
            };
        }

        public static Rental MapDtoToEntity(RentalCreateDto dto)
        {
            if (dto == null) return null!;
            return new Rental
            {
                UserId = dto.UserId.ToString(), // Conversione necessaria dato che in Entity Rental è string
                VehicleId = dto.VehicleId
            };
        }

        public static void UpdateEntity(Rental r, RentalUpdateDto dto)
        {
            if (dto == null || r == null) return;
            if (dto.StartDate.HasValue) r.StartDate = dto.StartDate;
            if (dto.EndDate.HasValue) r.EndDate = dto.EndDate;
            if (dto.TotalCost.HasValue) r.TotalCost = (decimal)dto.TotalCost.Value;
        }

    }
}