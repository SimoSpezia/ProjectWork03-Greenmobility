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
    [Authorize]
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
            var noleggi = await _ctx.Rentals
                .Include(r => r.Vehicle)
                .Include(r => r.User)
                .OrderByDescending(r => r.EndDate)
                .ToListAsync();

            return Ok(noleggi.ConvertAll(_mapper.MapEntityToDto));
        }

        // API POST che permette a un utente di prenotare un veicolo
        [HttpPost("prenotaMezzo")]
        [Authorize(Roles = Roles.CUSTOMER_ROLE)]
        public async Task<IActionResult> Prenota([FromBody] RentalCreateDto dto)
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
        [HttpPost("sbloccaMezzo")]
        [AllowAnonymous]
        public async Task<IActionResult> SbloccaMezzo([FromBody] RentalSbloccaDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.RentalCode)
                || dto.RentalCode.Length != 6
                || !dto.RentalCode.All(char.IsDigit))
                return BadRequest("Formato codice non valido. Deve essere di 6 cifre numeriche.");

            var authHeader = Request.Headers["Authorization"].FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                return Unauthorized("API Key del mezzo mancante nell'header della richiesta.");

            var tokenString = authHeader.Substring("Bearer ".Length).Trim();
            int idMezzoDalToken;

            try
            {
                var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(tokenString);
                var idString = jwtToken.Claims
                    .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

                if (!int.TryParse(idString, out idMezzoDalToken))
                    return Unauthorized("Impossibile estrarre l'ID del mezzo dall'API Key.");
            }
            catch
            {
                return Unauthorized("API Key del mezzo corrotta o non valida.");
            }

            var noleggio = await _ctx.Rentals
                .Include(r => r.Vehicle)
                .FirstOrDefaultAsync(r =>
                    r.RentalCode == dto.RentalCode
                    && r.StartDate == null
                    && r.EndDate == null);

            if (noleggio == null)
                return BadRequest("Codice inesistente, scaduto o già utilizzato.");

            if (noleggio.VehicleId != idMezzoDalToken)
                return BadRequest("Questo codice di sblocco appartiene a un altro veicolo.");

            if (noleggio.Vehicle != null && noleggio.Vehicle.VehicleStatusId != 3)
                return BadRequest("Il veicolo non si trova nello stato corretto per lo sblocco.");

            noleggio.StartDate = DateTimeOffset.UtcNow;
            noleggio.RentalCode = null;

            if (noleggio.Vehicle != null)
                noleggio.Vehicle.VehicleStatusId = 2;

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
                StartDate = noleggio.StartDate,
                VehicleId = noleggio.VehicleId
            });
        }

        // API PATCH che termina il noleggio ed effettua il calcolo della spesa e il rilascio del veicolo
        [HttpPatch("{id}/termina")]
        [Authorize(Roles = Roles.CUSTOMER_ROLE + "," + Roles.ADMIN_ROLE)]
        public async Task<IActionResult> Termina([FromRoute] int id, [FromBody] RentalUpdateDto dto)
        {
            var noleggio = await _ctx.Rentals
                .Include(r => r.Vehicle)
                .FirstOrDefaultAsync(r => r.RentalId == id && r.EndDate == null);

            if (noleggio == null)
                return NotFound("Noleggio non trovato o già terminato.");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            if (userRole == Roles.CUSTOMER_ROLE && noleggio.UserId != userId)
                return Forbid();

            if (!noleggio.StartDate.HasValue)
                return BadRequest("Impossibile terminare un noleggio non ancora avviato.");

            noleggio.EndDate = DateTimeOffset.UtcNow;

            var durata = noleggio.EndDate.Value - noleggio.StartDate.Value;
            var minuti = (decimal)durata.TotalMinutes;
            noleggio.TotalCost = Math.Round(minuti * 0.15m, 2);

            if (noleggio.Vehicle != null)
            {
                if (dto.BatteryLevel.HasValue)
                    noleggio.Vehicle.BatteryLevel = dto.BatteryLevel.Value;

                noleggio.Vehicle.VehicleStatusId = (noleggio.Vehicle.BatteryLevel < 15) ? 4 : 1;
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
                Costo = noleggio.TotalCost,
                Durata = durata.ToString(@"hh\:mm\:ss")
            });
        }
    }
}