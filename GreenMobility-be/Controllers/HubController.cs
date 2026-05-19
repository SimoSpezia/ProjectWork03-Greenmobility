using GreenMobility_be.Data;
using GreenMobility_be.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GreenMobility_be.Controllers
{
    [Route("api/hubs")]
    [ApiController]
    [Authorize]
    public class HubController : ControllerBase
    {
        private readonly GreenMobilityDbContext _ctx;

        public HubController(GreenMobilityDbContext ctx)
        {
            _ctx = ctx;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var hubs = _ctx.Hubs
                .Where(h => !h.IsDeleted)
                .Select(h => Mapper.MapEntityToDto(h))
                .ToList();

            return Ok(hubs);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var hub = _ctx.Hubs
                .Include(h => h.Vehicles)
                .SingleOrDefault(h => h.HubId == id && !h.IsDeleted);

            if (hub == null)
                return NotFound($"Hub con id {id} non trovato");

            return Ok(Mapper.MapEntityToDto(hub));
        }

        [HttpGet("{id}/vehicles")]
        public IActionResult GetVehiclesInHub(int id)
        {
            var hubEsiste = _ctx.Hubs.Any(h => h.HubId == id && !h.IsDeleted);
            if (!hubEsiste)
                return NotFound($"Hub con id {id} non trovato");

            var veicoli = _ctx.Vehicles
                .Where(v => v.HubId == id
                         && !v.IsDeleted
                         && v.VehicleStatus.Status == "Disponibile")
                .Select(v => Mapper.MapEntityToDto(v))
                .ToList();

            return Ok(veicoli);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Create([FromBody] HubCreateDto dto)
        {
            if (_ctx.Hubs.Any(h => h.Name == dto.Name && !h.IsDeleted))
                return BadRequest($"Esiste già un hub con il nome '{dto.Name}'");

            var hub = Mapper.MapDtoToEntity(dto);
            hub.IsDeleted = false;

            try
            {
                _ctx.Hubs.Add(hub);
                _ctx.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"Errore durante la creazione dell'hub: {ex.Message}");
            }

            return CreatedAtAction(
                nameof(GetById),
                new { id = hub.HubId },
                Mapper.MapEntityToDto(hub)
            );
        }

        [HttpPatch("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Update(int id, [FromBody] HubUpdateDto dto)
        {
            var hub = _ctx.Hubs.FirstOrDefault(h => h.HubId == id && !h.IsDeleted);
            if (hub == null)
                return NotFound($"Hub con id {id} non trovato");

            if (dto.Name != null) hub.Name = dto.Name;
            if (dto.Address != null) hub.Address = dto.Address;
            if (dto.City != null) hub.City = dto.City;
            if (dto.MaximumCapacity.HasValue) hub.MaximumCapacity = dto.MaximumCapacity.Value;

            try
            {
                _ctx.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"Errore durante la modifica dell'hub: {ex.Message}");
            }

            return Ok(Mapper.MapEntityToDto(hub));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var hub = _ctx.Hubs.FirstOrDefault(h => h.HubId == id && !h.IsDeleted);
            if (hub == null)
                return NotFound($"Hub con id {id} non trovato");

            hub.IsDeleted = true;

            try
            {
                _ctx.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"Errore durante la cancellazione dell'hub: {ex.Message}");
            }

            return NoContent();
        }
    }
}