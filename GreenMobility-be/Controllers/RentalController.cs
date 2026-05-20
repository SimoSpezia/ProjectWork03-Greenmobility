using GreenMobility_be.Data;
using GreenMobility_be.Dto;
using GreenMobility_be.Mapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GreenMobility_be.Controllers
{
    [Route("api/noleggi")]
    [ApiController]
    public class NoleggioController : ControllerBase
    {
        private readonly GreenMobilityDbContext _ctx;
        private readonly RentalMapper _mapper;

        public NoleggioController(GreenMobilityDbContext ctx, RentalMapper mapper)
        {
            _ctx = ctx;
            _mapper = mapper;
        }

        // API GET che recupera la lista completa di tutti i noleggi
        [HttpGet]
        [Authorize(Roles = Roles.ADMIN_ROLE)]
        public async Task<IActionResult> GetAll()
        {
            var rentals = await _ctx.Rentals
                .Include(r => r.Vehicle)
                .Include(r => r.User)
                .OrderByDescending(r => r.EndDate)
                .ToListAsync();

            return Ok(rentals.ConvertAll(_mapper.MapEntityToDto));
        }

        // API POST che permette a un utente di prenotare un veicolo
        [HttpPost("ReserveVehicle")]
        [Authorize(Roles = Roles.CUSTOMER_ROLE)]
        public async Task<IActionResult> Reserve([FromBody] RentalCreateDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var veicolo = await _ctx.Vehicles
                .Include(v => v.VehicleStatus)
                .FirstOrDefaultAsync(v => v.VehicleId == dto.VehicleId && !v.IsDeleted);

            if (veicolo == null || veicolo.VehicleStatus.VehicleStatusId != 1)
                return BadRequest("Veicolo non disponibile.");

            string codiceMonouso;
            bool codiceEsistente;
            do
            {
                codiceMonouso = Random.Shared.Next(100000, 1000000).ToString();
                codiceEsistente = await _ctx.Rentals
                    .AnyAsync(r => r.RentalCode == codiceMonouso && r.EndDate == null);
            } while (codiceEsistente);

            var noleggio = new Rental
            {
                UserId = userId,
                VehicleId = dto.VehicleId,
                RentalCode = codiceMonouso
            };

            try
            {
                _ctx.Rentals.Add(noleggio);
                veicolo.VehicleStatusId = 3;
                await _ctx.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"Errore durante la prenotazione: {ex.Message}");
            }

            return Ok(new
            {
                Messaggio = "Prenotazione effettuata",
                Codice = codiceMonouso,
                NoleggioId = noleggio.RentalId
            });
        }

        // API POST che sblocca il veicolo prenotato validando il codice monouso
        [HttpPost("UnlockVehicle")]
        [AllowAnonymous]
        public async Task<IActionResult> UnlockVehicle([FromBody] RentalUnlockDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.RentalCode) || dto.RentalCode.Length != 6 || !dto.RentalCode.All(char.IsDigit))
                return BadRequest("Formato codice non valido. Deve essere di 6 cifre numeriche.");

            if (!Request.Headers.TryGetValue("ApiKey", out var extractedApiKey))
                return Unauthorized("API Key del mezzo mancante.");

            var apiKeyString = extractedApiKey.ToString().Trim();

            if (!Guid.TryParse(apiKeyString, out var parsedGuid))
                return BadRequest("Formato API Key non valido.");

            var physicalVehicle = await _ctx.Vehicles.FirstOrDefaultAsync(v => v.ApiKey == parsedGuid);

            if (physicalVehicle == null)
                return Unauthorized("API Key non riconosciuta o veicolo inesistente.");

            var rental = await _ctx.Rentals
                .FirstOrDefaultAsync(r => r.RentalCode == dto.RentalCode && r.StartDate == null && r.EndDate == null);

            if (rental == null)
                return BadRequest("Codice inesistente, scaduto o già utilizzato.");

            if (rental.VehicleId != physicalVehicle.VehicleId)
                return BadRequest("Questo codice di sblocco appartiene a un altro veicolo.");

            if (physicalVehicle.VehicleStatusId != 3)
                return BadRequest("Il veicolo non si trova nello stato corretto per lo sblocco.");

            rental.StartDate = DateTimeOffset.UtcNow;
            rental.RentalCode = null;
            physicalVehicle.VehicleStatusId = 2;

            try
            {
                await _ctx.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"Errore durante l'aggiornamento del database: {ex.Message}");
            }

            return Ok(new
            {
                Messaggio = "Veicolo sbloccato, buon viaggio!",
                StartDate = rental.StartDate,
                VehicleId = physicalVehicle.VehicleId
            });
        }

        // API PATCH che termina il noleggio ed effettua il calcolo della spesa e il rilascio del veicolo
        [HttpPatch("endrental")]
        [AllowAnonymous]
        public async Task<IActionResult> EndRental([FromBody] RentalUpdateDto dto)
        {
            if (!Request.Headers.TryGetValue("ApiKey", out var extractedApiKey))
                return Unauthorized("API Key del mezzo mancante nell'header.");

            var apiKeyString = extractedApiKey.ToString().Trim();

            if (!Guid.TryParse(apiKeyString, out var parsedGuid))
                return BadRequest("Formato API Key non valido.");

            var physicalVehicle = await _ctx.Vehicles.FirstOrDefaultAsync(v => v.ApiKey == parsedGuid);

            if (physicalVehicle == null)
                return Unauthorized("API Key non riconosciuta o veicolo inesistente.");

            var rental = await _ctx.Rentals
                .Include(r => r.Vehicle)
                .FirstOrDefaultAsync(r => r.VehicleId == physicalVehicle.VehicleId && r.EndDate == null);

            if (rental == null)
                return NotFound("Nessun noleggio attivo trovato per questo veicolo.");

            if (!rental.StartDate.HasValue)
                return BadRequest("Impossibile terminare un noleggio non ancora avviato.");

            rental.EndDate = DateTimeOffset.UtcNow;
            var duration = rental.EndDate.Value - rental.StartDate.Value;
            var minutes = (decimal)duration.TotalMinutes;

            rental.TotalCost = Math.Round(Math.Max(1, minutes) * 0.20m, 2);
             
            if (rental.Vehicle != null && dto.BatteryLevel.HasValue)
            {
                rental.Vehicle.BatteryLevel = dto.BatteryLevel.Value;
                rental.Vehicle.VehicleStatusId = (rental.Vehicle.BatteryLevel < 15) ? 4 : 1;
            }

            try
            {
                await _ctx.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"Errore durante la chiusura del noleggio: {ex.Message}");
            }

            return Ok(new
            {
                Messaggio = "Noleggio terminato",
                Costo = rental.TotalCost,
                Durata = duration.ToString(@"hh\:mm\:ss"),
                BatteriaResidua = dto.BatteryLevel
            });
        }
    }
}