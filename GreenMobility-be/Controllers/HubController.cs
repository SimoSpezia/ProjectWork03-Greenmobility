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

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll()
        {
            var hubs = await _ctx.Hubs
                       .Where(h => !h.IsDeleted)
                       .ToListAsync();

            return Ok(hubs.ConvertAll(_mapper.MapEntityToDto));
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var hub = await _ctx.Hubs
                .Where(h => !h.IsDeleted)
                .Include(h => h.Vehicles.Where(v => !v.IsDeleted && v.VehicleStatus.Status == "Disponibile"))
                .SingleOrDefaultAsync(h => h.HubId == id);

            if (hub == null)
                return NotFound($"Hub con id {id} non trovato");

            return Ok(_mapper.MapEntityToDto(hub));
        }

        [HttpPost]
        [Authorize(Roles = Roles.ADMIN_ROLE)]
        public async Task<IActionResult> Create([FromBody] HubCreateDto dto)
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
                nameof(GetById),
                new { id = hub.HubId },
                _mapper.MapEntityToDto(hub)
            );
        }

        [HttpPatch("{id}")]
        [Authorize(Roles = Roles.ADMIN_ROLE)]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] HubUpdateDto dto)
        {
            var hub = await _ctx.Hubs.FirstOrDefaultAsync(h => h.HubId == id && !h.IsDeleted);
            if (hub == null)
                return NotFound($"Hub con id {id} non trovato");

            if (dto.Name != null) hub.Name = dto.Name;
            if (dto.Address != null) hub.Address = dto.Address;
            if (dto.City != null) hub.City = dto.City;
            if (dto.MaximumCapacity.HasValue) hub.MaximumCapacity = dto.MaximumCapacity.Value;

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

        [HttpDelete("{id}")]
        [Authorize(Roles = Roles.ADMIN_ROLE)]
        public async Task<IActionResult> Delete([FromRoute] int id)
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