using System.ComponentModel.DataAnnotations;

namespace GreenMobility_be.Dto
{
    public class RentalCreateDto
    {
        public required string UserId { get; set; } // L'id dell'utente che sta noleggiando
        public int VehicleId { get; set; } // L'unica cosa che il cliente sceglie è il mezzo
    }
}