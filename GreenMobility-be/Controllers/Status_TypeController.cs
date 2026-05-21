using GreenMobility_be.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GreenMobility_be.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Status_TypeController(GreenMobilityDbContext ctx) : ControllerBase
    {

        private readonly GreenMobilityDbContext _ctx = ctx;

        [HttpGet]
        [Route("GetAllVehicleStatuses")]
        //[Authorize(Roles = Roles.OPERATOR_ROLE + "," + Roles.ADMIN_ROLE)]
        public async Task<IActionResult> StatusesGetAll()
        {
            var statuses = (from vs in _ctx.VehicleStatuses
                          select new {vs.VehicleStatusId, vs.Status}).ToList();
            return Ok(statuses);
        }

        [HttpGet]
        [Route("GetAllVehicleTypes")]
        //[Authorize(Roles = Roles.OPERATOR_ROLE + "," + Roles.ADMIN_ROLE)]
        public async Task<IActionResult> TypesGetAll()
        {
            var types = (from vt in _ctx.VehicleTypes
                         select new { vt.VehicleTypeId, vt.Type }).ToList();
            return Ok(types);
        }
    }
}