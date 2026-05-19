using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace GreenMobility_be.Data
{
    public class User : IdentityUser
    {
        [MaxLength(100)]
        public required string Name { get; set; }
        [MaxLength(100)]
        public required string Surname { get; set; }    
        public List<Rental>? Rentals { get; set; }
        public bool IsDeleted{ get; set; }=false;
    }
}
