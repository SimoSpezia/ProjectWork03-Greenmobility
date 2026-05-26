using GreenMobility_be.Data;
using GreenMobility_be.Dto;
using GreenMobility_be.Mapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GreenMobility_be.Controllers
{
    [Route("api/hubs")]
    [ApiController]

    public class HubController(GreenMobilityDbContext ctx, HubMapper mapper) : ControllerBase
    {
        private readonly GreenMobilityDbContext _ctx = ctx;
        private readonly HubMapper _mapper = mapper;

        /// <summary>Restituisce la lista di tutti gli hub non eliminati.</summary>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllHubs()
        {
            var hubs = await _ctx.Hubs
                       .Where(h => !h.IsDeleted)
                       .ToListAsync();

            return Ok(hubs.ConvertAll(_mapper.MapEntityToDto));
        }

        /// <summary>Restituisce il dettaglio di un hub con i veicoli disponibili.</summary>
        /// <param name="id">ID dell'hub da recuperare.</param>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetHubById([FromRoute] int id)
        {
            var hub = await _ctx.Hubs
                .Where(h => !h.IsDeleted)
                .Include(h => h.Vehicles.Where(v => !v.IsDeleted && v.VehicleStatus.Status == "Disponibile"))
                .ThenInclude(v => v.VehicleStatus)
                .Include(h => h.Vehicles.Where(v => !v.IsDeleted && v.VehicleStatus.Status == "Disponibile"))
                .ThenInclude(v => v.VehicleType)
                .SingleOrDefaultAsync(h => h.HubId == id);

            if (hub == null)
                return NotFound($"Hub con id {id} non trovato");

            return Ok(_mapper.MapEntityToDto(hub));
        }

        /// <summary>Crea un nuovo hub. Solo admin.</summary>
        /// <param name="dto">Dati del nuovo hub (nome, indirizzo, città, capacità massima).</param>
        [HttpPost]
        [Authorize(Roles = Roles.ADMIN_ROLE)]
        public async Task<IActionResult> CreateHub([FromBody] HubCreateDto dto)
        {
            if (await _ctx.Hubs.AnyAsync(h => h.Name == dto.Name && !h.IsDeleted))
                return BadRequest($"Esiste già un hub con il nome '{dto.Name}'");

            var hub = _mapper.MapDtoToEntity(dto);
            hub.IsDeleted = false;

            try
            {
                _ctx.Hubs.Add(hub);
                await _ctx.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"Errore durante la creazione dell'hub: {ex.Message}");
            }

            return CreatedAtAction(
                nameof(GetHubById),
                new { id = hub.HubId },
                _mapper.MapEntityToDto(hub)
            );
        }

        /// <summary>Imposta in manutenzione tutti i veicoli con batteria scarica nell'hub tramite stored procedure. Solo admin.</summary>
        /// <param name="id">ID dell'hub su cui eseguire l'operazione.</param>
        [HttpPatch]
        [Authorize(Roles = Roles.ADMIN_ROLE)]
        [Route("{id}/maintenance-battery")]
        public async Task<IActionResult> SetMaintenanceBatteryByHubId([FromRoute] int id)
        {
            var hubExists = await _ctx.Hubs.AnyAsync(h => h.HubId == id && !h.IsDeleted);
            int vehiclesSet = 0;
            if (!hubExists) return NotFound($"Hub con id {id} non trovato o eliminato");
            try
            {
                vehiclesSet = await _ctx.Database.ExecuteSqlRawAsync("EXEC [dbo].[sp_set_vehiclestatus_manutenzione] @HubId = {0}", id);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore: {ex.Message}");
            }
            return Ok(new
            {
                Message = $"{vehiclesSet} veicoli sono stati impostati in manutenzione nell'hub {id}.",
            });
        }

        /// <summary>Aggiorna i dati di un hub esistente. Solo admin.</summary>
        /// <param name="id">ID dell'hub da modificare.</param>
        /// <param name="dto">Campi da aggiornare (nome, indirizzo, città, capacità massima).</param>
        [HttpPatch("{id}")]
        [Authorize(Roles = Roles.ADMIN_ROLE)]
        public async Task<IActionResult> UpdateHubById([FromRoute] int id, [FromBody] HubUpdateDto dto)
        {
            var hub = await _ctx.Hubs.FirstOrDefaultAsync(h => h.HubId == id && !h.IsDeleted);
            if (hub == null)
                return NotFound($"Hub con id {id} non trovato");

            // 1. Controllo validità Nome Univoco
            if (!string.IsNullOrEmpty(dto.Name) && dto.Name != hub.Name)
            {
                if (await _ctx.Hubs.AnyAsync(h => h.Name == dto.Name && !h.IsDeleted))
                    return BadRequest($"Esiste già un hub con il nome '{dto.Name}'");
                hub.Name = dto.Name;
            }

            if (!string.IsNullOrEmpty(dto.Address)) hub.Address = dto.Address;
            if (!string.IsNullOrEmpty(dto.City)) hub.City = dto.City;

            // 2. Controllo logico sulla Capacità Massima
            if (dto.MaximumCapacity.HasValue)
            {
                if (dto.MaximumCapacity.Value <= 0)
                    return BadRequest("La capacità massima deve essere un valore positivo.");

                int currentVehicleCount = await _ctx.Vehicles.CountAsync(v => v.HubId == id && !v.IsDeleted);
                if (dto.MaximumCapacity.Value < currentVehicleCount)
                {
                    return BadRequest($"La capacità massima ({dto.MaximumCapacity.Value}) non può essere inferiore al numero attuale di veicoli presenti in questo hub ({currentVehicleCount}).");
                }

                hub.MaximumCapacity = dto.MaximumCapacity.Value;
            }

            try
            {
                await _ctx.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"Errore durante la modifica dell'hub: {ex.Message}");
            }

            return Ok(_mapper.MapEntityToDto(hub));
        }

        /// <summary>Esegue il soft delete di un hub (non viene cancellato fisicamente). Solo admin.</summary>
        /// <param name="id">ID dell'hub da eliminare.</param>
        [HttpDelete("{id}")]
        [Authorize(Roles = Roles.ADMIN_ROLE)]
        public async Task<IActionResult> SoftDeleteHubById([FromRoute] int id)
        {
            var hub = await _ctx.Hubs.FirstOrDefaultAsync(h => h.HubId == id && !h.IsDeleted);
            if (hub == null)
                return NotFound($"Hub con id {id} non trovato");
            hub.IsDeleted = true;
            try
            {
                await _ctx.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"Errore durante la cancellazione dell'hub: {ex.Message}");
            }
            return NoContent();
        }
    }
}