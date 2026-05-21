
namespace GreenMobility_be.Dto
{
    public class HubDto
    {
        public int Id { get; set; }

        public required string Name { get; set; }

        public required string Address { get; set; }

        public required string City { get; set; }
        public int MaximumCapacity { get; set; }
        public List<VehicleDto>? Vehicles { get; set; }

    }
}
