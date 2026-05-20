using GreenMobility_be.Data;
using GreenMobility_be.Dto;

namespace GreenMobility_be.Mapper
{
    public class RentalMapper
    {
        public RentalDto MapEntityToDto(Rental r)
        {
            if (r == null) return null!;
            return new RentalDto
            {
                Id = r.RentalId,
                UserId = r.UserId,
                VehicleId = r.VehicleId,
                StartDate = r.StartDate,
                EndDate = r.EndDate,
                TotalCost = r.TotalCost,
                RentalCode = r.RentalCode
            };
        }

        public Rental MapDtoToEntity(RentalCreateDto dto)
        {
            if (dto == null) return null!;
            return new Rental
            {
                UserId = dto.UserId.ToString(),
                VehicleId = dto.VehicleId
            };
        }

        public void UpdateEntity(Rental r, RentalUpdateDto dto)
        {
            if (dto == null || r == null) return;
            if (dto.StartDate.HasValue) r.StartDate = dto.StartDate;
            if (dto.EndDate.HasValue) r.EndDate = dto.EndDate;
            if (dto.TotalCost.HasValue) r.TotalCost = (decimal)dto.TotalCost.Value;
        }
    }
}