using GreenMobility_be.Data;
using GreenMobility_be.Dto;
using GreenMobility_be.Mapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GreenMobility_be.Controllers
{
    [Route("api/vehicles")]
    [ApiController]
    public class VehicleController(GreenMobilityDbContext ctx, VehicleMapper mapper) : ControllerBase
    {
        private readonly GreenMobilityDbContext _ctx = ctx;
        private readonly VehicleMapper _mapper = mapper;


        // FUNZIONALITÀ ADMIN

        /// <summary>
        /// Creazione di un veicolo, possibile solo per l'admin.
        /// </summary>
        /// <param name="dto">Dati del nuovo veicolo: tipo (1=E-Bike, 2=Monopattino) e ID hub di appartenenza.</param>
        [HttpPost]
        [Authorize(Roles = Roles.ADMIN_ROLE)]
        public async Task<IActionResult> CreateVehicle(VehicleCreateDto dto)
        {
            var hub = await _ctx.Hubs.FirstOrDefaultAsync(h => h.HubId == dto.HubId);
            var typeExists = await _ctx.VehicleTypes.AnyAsync(t => t.VehicleTypeId == dto.VehicleTypeId);

            if (hub == null || !typeExists)
            {
                return BadRequest(new { Message = "Uno o più ID specificati (Hub o Tipo) non sono validi." });
            }

            var currentVehicleCount = await _ctx.Vehicles.CountAsync(v => v.HubId == dto.HubId && !v.IsDeleted);
            if (currentVehicleCount >= hub.MaximumCapacity)
            {
                return BadRequest(new { Message = "L'Hub selezionato ha già raggiunto la capacità massima." });
            }

            string newUIC = dto.VehicleTypeId switch
            {
                1 => "B" + (await _ctx.Vehicles.CountAsync(v => v.VehicleTypeId == 1) + 1).ToString(),
                2 => "M" + (await _ctx.Vehicles.CountAsync(v => v.VehicleTypeId == 2) + 1).ToString()
            };

            var newVehicle = new Vehicle
            {
                VehicleTypeId = dto.VehicleTypeId,
                VehicleStatusId = 1,
                HubId = dto.HubId,
                UIC = newUIC,
                BatteryLevel = 100,
                ApiKey = Guid.NewGuid()
            };

            _ctx.Vehicles.Add(newVehicle);
            try
            {
                await _ctx.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"Errore durante la creazione del veicolo: {ex.Message}");
            }

            newVehicle.VehicleType = await _ctx.VehicleTypes.FindAsync(newVehicle.VehicleTypeId);
            newVehicle.VehicleStatus = await _ctx.VehicleStatuses.FindAsync(newVehicle.VehicleStatusId);
            newVehicle.Hub = await _ctx.Hubs.FindAsync(newVehicle.HubId);

            var resultDto = _mapper.MapEntityToDto(newVehicle);

            return CreatedAtAction(
            nameof(GetVehicleById),
            new { id = newVehicle.VehicleId },
            new
            {
                Vehicle = resultDto,
                ApiKey = newVehicle.ApiKey
            });
        }


        /// <summary>
        /// Lista di tutti i veicoli, possibile solo per l'admin.
        /// </summary>
        [HttpGet]
        [Authorize(Roles = Roles.ADMIN_ROLE)]
        public async Task<IActionResult> GetAllVehicles()
        {
            var entities = await _ctx.Vehicles
            .Where(v => !v.IsDeleted)
            .Include(v => v.VehicleType)
            .Include(v => v.VehicleStatus)
            .Include(v => v.Hub).Where(h=>!h.IsDeleted)
            .ToListAsync();

            var dtos = entities.ConvertAll(_mapper.MapEntityToDto);

            return Ok(dtos);
        }


        /// <summary>
        /// Modifica di un veicolo, possibile solo per l'admin.
        /// </summary>
        /// <param name="id">ID del veicolo da modificare.</param>
        /// <param name="dto">Campi da aggiornare: hubId, tipoVeicolo, stato e/o livello batteria (tutti opzionali).</param>
        [HttpPatch]
        [Authorize(Roles = Roles.ADMIN_ROLE)]
        [Route("{id}")]
        public async Task<IActionResult> UpdateVehicleById(int id, VehicleUpdateDto dto)
        {
            var vehicle = await _ctx.Vehicles.FindAsync(id);
            if (vehicle == null)
            {
                return NotFound(new { Message = $"Veicolo con ID '{id}' non trovato." });
            }

            if (dto.HubId.HasValue && dto.HubId.Value > 0)
            {
                var hub = await _ctx.Hubs.FirstOrDefaultAsync(h => h.HubId == dto.HubId.Value);
                if (hub == null)
                    return BadRequest(new { Message = "L'Hub specificato non è valido." });

                if (vehicle.HubId != dto.HubId.Value)
                {
                    var currentVehicleCount = await _ctx.Vehicles.CountAsync(v => v.HubId == dto.HubId.Value && !v.IsDeleted);
                    if (currentVehicleCount >= hub.MaximumCapacity)
                    {
                        return BadRequest(new { Message = "L'Hub selezionato ha già raggiunto la capacità massima." });
                    }
                }

                vehicle.HubId = dto.HubId.Value;
            }

            if (dto.VehicleTypeId.HasValue)
            {
                if (dto.VehicleTypeId.Value != 1 && dto.VehicleTypeId.Value != 2)
                {
                    return BadRequest(new { Message = "Tipologia veicolo non valida. Sono ammessi solo i valori 1 (E-Bike) o 2 (Monopattino)." });
                }

                var typeExists = await _ctx.VehicleTypes.AnyAsync(t => t.VehicleTypeId == dto.VehicleTypeId.Value);
                if (!typeExists)
                    return BadRequest(new { Message = "La tipologia di veicolo specificata non è valida." });

                vehicle.VehicleTypeId = dto.VehicleTypeId.Value;
            }

            if (dto.VehicleStatusId.HasValue)
            {
                var statusExists = await _ctx.VehicleStatuses.AnyAsync(s => s.VehicleStatusId == dto.VehicleStatusId.Value);
                if (!statusExists)
                    return BadRequest(new { Message = "Lo stato specificato non è valido." });

                vehicle.VehicleStatusId = dto.VehicleStatusId.Value;
            }

            if (dto.BatteryLevel.HasValue)
            {

                if (dto.BatteryLevel.Value < 0 || dto.BatteryLevel.Value > 100)
                {
                    return BadRequest(new { Message = "Il livello della batteria deve essere compreso tra 0 e 100." });
                }

                vehicle.BatteryLevel = dto.BatteryLevel.Value;
            }

            await _ctx.SaveChangesAsync();
            return NoContent();
        }


        /// <summary>
        /// Sospensione di un veicolo, possibile solo per l'admin.
        /// </summary>
        /// <param name="id">ID del veicolo da sospendere.</param>
        [HttpPatch]
        [Authorize(Roles = Roles.ADMIN_ROLE)]
        [Route("{id}/soft-delete")]
        public async Task<IActionResult> SoftDeleteVehicleById(int id)
        {
            var vehicle = await _ctx.Vehicles.FindAsync(id);
            if (vehicle == null)
            {
                return NotFound(new { Message = $"Veicolo con ID '{id}' non trovato." });
            }
            if (vehicle.IsDeleted)
            {
                return BadRequest(new { Message = $"Veicolo con ID '{id}' già sospeso." });
            }
            if(vehicle.VehicleStatusId == 2)
            {
                return BadRequest(new { Message = $"Veicolo con ID '{id}' attualmente in noleggio. Impossibile sospendere." });
            }

            vehicle.IsDeleted = true;
            await _ctx.SaveChangesAsync();
            return NoContent();
        }


        // FUNZIONALITÀ OPERATORE

        /// <summary>
        /// Lista di veicoli per l'operatore: mostra solo i veicoli in manutenzione o con batteria scarica.
        /// </summary>
        [HttpGet]
        [Authorize(Roles = Roles.OPERATOR_ROLE)]
        [Route("maintenance-list")]
        public async Task<IActionResult> GetAllVehiclesInMaintenance()
        {
            int lowBatteryThreshold = 20;

            var entities = await _ctx.Vehicles
            .Include(v => v.VehicleType)
            .Include(v => v.VehicleStatus)
            .Include(v => v.Hub)
            .Where(v => !v.IsDeleted && (v.VehicleStatusId == 3 || v.BatteryLevel <= lowBatteryThreshold))
            .ToListAsync();

            var dtos = entities.ConvertAll(_mapper.MapEntityToDto);

            return Ok(dtos);
        }


        /// <summary>
        /// Modifica stato veicolo e/o livello batteria, possibile solo per l'operatore.
        /// </summary>
        /// <param name="id">ID del veicolo da aggiornare.</param>
        /// <param name="batteryLevel">Nuovo livello batteria (0–100). Opzionale.</param>
        /// <param name="statusId">ID del nuovo stato del veicolo. Opzionale.</param>
        [HttpPatch]
        [Authorize(Roles = Roles.OPERATOR_ROLE)]
        [Route("{id}/maintain-vehicle")]
        public async Task<IActionResult> MaintainVehicleById(int id, int? batteryLevel, int? statusId)
        {
            if (!batteryLevel.HasValue && !statusId.HasValue)
            {
                return BadRequest(new { Message = "È necessario specificare almeno un parametro da modificare (batteryLevel o statusId)." });
            }

            var vehicle = await _ctx.Vehicles.FindAsync(id);
            if (vehicle == null || vehicle.IsDeleted)
            {
                return NotFound(new { Message = $"Veicolo con ID '{id}' non trovato o eliminato." });
            }

            if (batteryLevel.HasValue)
            {
                if (batteryLevel.Value < 0 || batteryLevel.Value > 100)
                {
                    return BadRequest(new { Message = "Il livello della batteria deve essere un valore compreso tra 0 e 100." });
                }

                vehicle.BatteryLevel = batteryLevel.Value;
            }

            if (statusId.HasValue)
            {
                var statusExists = await _ctx.VehicleStatuses.AnyAsync(s => s.VehicleStatusId == statusId.Value);
                if (!statusExists)
                {
                    return BadRequest(new { Message = $"Lo stato con ID '{statusId.Value}' non esiste nel sistema." });
                }

                vehicle.VehicleStatusId = statusId.Value;
            }

            await _ctx.SaveChangesAsync();

            return NoContent();
        }


        /// <summary>
        /// Mostra la lista di tutti gli stati possibili dei veicoli. 
        /// Serve al frontend per popolare le tendine di cambio stato rapido.
        /// </summary>
        [HttpGet]
        [Authorize(Roles = Roles.OPERATOR_ROLE)]
        [Route("statuses-list")]
        public async Task<IActionResult> GetAllVehicleStatuses()
        {
            var statuses = await _ctx.VehicleStatuses
                .Select(s => new { s.VehicleStatusId, s.Status })
                .ToListAsync();

            return Ok(statuses);
        }


        // FUNZIONALITÀ ADMIN E OPERATORE

        /// <summary>
        /// Dettaglio di un veicolo, possibile sia per l'admin che per l'operatore.
        /// </summary>
        /// <param name="id">ID del veicolo da recuperare.</param>
        [HttpGet]
        [Authorize]
        [Route("{id}")]
        public async Task<IActionResult> GetVehicleById(int id)
        {
            var vehicle = await _ctx.Vehicles
            .Where(v => !v.IsDeleted)
            .Include(v => v.VehicleType)
            .Include(v => v.VehicleStatus)
            .Include(v => v.Hub)
            .FirstOrDefaultAsync(v => v.VehicleId == id);

            if (vehicle == null)
            {
                return NotFound(new { Message = $"Veicolo con ID '{id}' non trovato." });
            }

            var dto = _mapper.MapEntityToDto(vehicle);

            return Ok(dto);
        }
    }
}
