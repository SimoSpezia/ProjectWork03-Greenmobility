namespace GreenMobility_be.Dto
{
    public class RentalDto
    {
        public int Id { get; set; } // Necessario per identificare il noleggio
        public string UserId { get; set; } = null!;
        public int VehicleId { get; set; }
        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
        public decimal? TotalCost { get; set; }
        public string? RentalCode { get; set; } // Fondamentale per il pairing bici-utente

    }
}