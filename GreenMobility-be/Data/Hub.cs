using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace GreenMobility_be.Data
{
    public class Hub
    {
        public int HubId { get; set; }
        [MaxLength(100)]
        public required string Name { get; set; }
        [MaxLength(100)]
        public required string Address { get; set; }
        [MaxLength(100)]
        public required string City { get; set; }
        public int MaximumCapacity { get; set; } 
        public List<Vehicle>? Vehicles { get; set; }
        public bool IsDeleted { get; set; }=false;
    }
}
