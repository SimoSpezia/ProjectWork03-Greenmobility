using System.ComponentModel.DataAnnotations;

namespace GreenMobility_be.Dto
{
    public class RentalCreateDto
    {
        public required int UserId { get; set; } // L'id dell'utente che sta noleggiando
        public required int VehicleId { get; set; } // L'unica cosa che il cliente sceglie è il mezzo
    }
}