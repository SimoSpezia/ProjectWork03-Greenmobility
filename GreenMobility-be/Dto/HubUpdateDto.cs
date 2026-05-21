using System.ComponentModel.DataAnnotations;

namespace GreenMobility_be.Dto
{
    public class HubUpdateDto
    {
        [MaxLength(100)]
        public  string? Name { get; set; }
        [MaxLength(100)]
        public  string? Address { get; set; }
        [MaxLength(100)]
        public  string? City { get; set; }
        [Range(5,75)]
        public int? MaximumCapacity { get; set; }
    }
}
