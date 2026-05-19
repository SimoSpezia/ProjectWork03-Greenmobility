using GreenMobility_be.Data;
using GreenMobility_be.Dto;
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

        public NoleggioController(GreenMobilityDbContext ctx)
        {
            _ctx = ctx;
        }

        [HttpGet]
        [Authorize(Roles = Roles.ADMIN_ROLE)]
        public IActionResult GetAll()
        {
            var noleggi = _ctx.Rentals
                .Include(r => r.Vehicle)
                .Include(r => r.User)
                .OrderByDescending(r => r.EndDate)
                .Select(r => Mapper.MapEntityToDto(r))
                .ToList();

            return Ok(noleggi);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = Roles.ADMIN_ROLE)]
        public IActionResult GetById(int id)
        {
            var noleggio = _ctx.Rentals
                .Include(r => r.Vehicle)
                .Include(r => r.User)
                .SingleOrDefault(r => r.RentalId == id);

            if (noleggio == null)
                return NotFound($"Noleggio con id {id} non trovato");

            return Ok(Mapper.MapEntityToDto(noleggio));
        }

        [HttpGet("utente/{id}")]
        [Authorize(Roles = Roles.ADMIN_ROLE + "," + Roles.CUSTOMER_ROLE)]
        public IActionResult GetUtenteById(string id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);

            if (currentUserRole == Roles.CUSTOMER_ROLE && currentUserId != id)
                return Forbid();

            var noleggi = _ctx.Rentals
                .Include(r => r.Vehicle)
                .Include(r => r.User)
                .Where(r => r.UserId == id)
                .OrderByDescending(r => r.EndDate)
                .Select(r => Mapper.MapEntityToDto(r))
                .ToList();

            return Ok(noleggi);
        }

        [HttpGet("attivo")]
        [Authorize(Roles = Roles.CUSTOMER_ROLE)]
        public IActionResult GetAttivo()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var noleggioAttivo = _ctx.Rentals
                .Include(r => r.Vehicle)
                .Include(r => r.User)
                .Where(r => r.UserId == userId && r.EndDate == null)
                .OrderByDescending(r => r.RentalId)
                .FirstOrDefault();

            if (noleggioAttivo == null)
                return NotFound("Nessun noleggio attivo per l'utente corrente.");

            return Ok(Mapper.MapEntityToDto(noleggioAttivo));
        }

        [HttpPost("prenotaMezzo")]
        [Authorize(Roles = Roles.CUSTOMER_ROLE)]
        public IActionResult Prenota([FromBody] RentalCreateDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var veicolo = _ctx.Vehicles
                .Include(v => v.VehicleStatus)
                .FirstOrDefault(v => v.VehicleId == dto.VehicleId && !v.IsDeleted);

            if (veicolo == null || veicolo.VehicleStatus.VehicleStatusId != 1)
                return BadRequest("Veicolo non disponibile.");

            string codiceMonouso;
            bool codiceEsistente;

            do
            {
                codiceMonouso = Random.Shared.Next(100000, 1000000).ToString();
                codiceEsistente = _ctx.Rentals.Any(r => r.RentalCode == codiceMonouso && r.EndDate == null);
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
                veicolo.VehicleStatusId = 3; // Aggiornato a "Prenotato"
                _ctx.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"Errore durante la prenotazione: {ex.Message}");
            }

            return Ok(new { Messaggio = "Prenotazione effettuata", Codice = codiceMonouso, NoleggioId = noleggio.RentalId });
        }

        [HttpPost("sbloccaMezzo")]
        [AllowAnonymous]
        public IActionResult SbloccaMezzo([FromBody] string codice)
        {
            if (string.IsNullOrWhiteSpace(codice) || codice.Length != 6 || !codice.All(char.IsDigit))
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

                var idString = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

                if (!int.TryParse(idString, out idMezzoDalToken))
                    return Unauthorized("Impossibile estrarre l'ID del mezzo dall'API Key.");
            }
            catch
            {
                return Unauthorized("API Key del mezzo corrotta o non valida.");
            }

            var noleggio = _ctx.Rentals
                .Include(r => r.Vehicle)
                .FirstOrDefault(r => r.RentalCode == codice && r.StartDate == null && r.EndDate == null);

            if (noleggio == null)
                return BadRequest("Codice inesistente, scaduto o già utilizzato.");

            if (noleggio.VehicleId != idMezzoDalToken)
                return BadRequest("Questo codice di sblocco appartiene a un altro veicolo. Hai sbagliato bici!");

            if (noleggio.Vehicle != null && noleggio.Vehicle.VehicleStatusId != 3) // Deve essere prenotato per essere sbloccato
                return BadRequest("Il veicolo non si trova nello stato corretto per lo sblocco.");

            noleggio.StartDate = DateTimeOffset.UtcNow;
            noleggio.RentalCode = null;

            if (noleggio.Vehicle != null)
            {
                noleggio.Vehicle.VehicleStatusId = 2; // Stato "In uso"
            }

            try
            {
                _ctx.SaveChanges();
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

        [HttpPatch("{id}/termina")]
        [Authorize(Roles = Roles.CUSTOMER_ROLE + "," + Roles.ADMIN_ROLE)]
        public IActionResult Termina(int id, [FromBody] RentalUpdateDto dto)
        {
            var noleggio = _ctx.Rentals
                .Include(r => r.Vehicle)
                .FirstOrDefault(r => r.RentalId == id && r.EndDate == null);

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
            decimal tariffaAlMinuto = 0.15m;
            noleggio.TotalCost = Math.Round(minuti * tariffaAlMinuto, 2);

            if (noleggio.Vehicle != null)
            {
                if (dto.BatteryLevel.HasValue)
                    noleggio.Vehicle.BatteryLevel = dto.BatteryLevel.Value;

                noleggio.Vehicle.VehicleStatusId = (noleggio.Vehicle.BatteryLevel < 15) ? 4 : 1; // 4 = Manutenzione, 1 = Disponibile
            }

            try
            {
                _ctx.SaveChanges();
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