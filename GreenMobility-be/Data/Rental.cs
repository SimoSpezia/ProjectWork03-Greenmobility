using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GreenMobility_be.Data
{
    [Index(nameof(RentalCode), IsUnique = true)]
    public class Rental
    {
        public int RentalId { get; set; }
        public required string UserId { get; set; }
        public int VehicleId { get; set; }
        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
        [Column(TypeName = "decimal(5,2)")]
        public decimal? TotalCost { get; set; }
        [StringLength(6)]
        public string? RentalCode { get; set; }
        public User? User { get; set; }
        public Vehicle? Vehicle { get; set; }
    }
}