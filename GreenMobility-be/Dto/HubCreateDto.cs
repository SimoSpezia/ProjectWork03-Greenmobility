using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GreenMobility_be.Dto
{
    public class HubCreateDto
    {
        [JsonIgnore]
        public int HubId { get; set; }
        [MaxLength(100)]
        public required string Name { get; set; }
        [MaxLength(100)]
        public required string Address { get; set; }
        [MaxLength(100)]
        public required string City { get; set; }
        public int MaximumCapacity { get; set; }


    }
}
