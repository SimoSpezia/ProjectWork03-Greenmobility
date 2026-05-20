using GreenMobility_be.Data;
using GreenMobility_be.Dto;
using GreenMobility_be.Mapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GreenMobility_be.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, UserMapper mapper) : ControllerBase
    {
        private readonly UserManager<User> _userManager = userManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;
        private readonly UserMapper _mapper = mapper;

        /// <summary>
        /// Lista di tutti gli utenti nel sistema. Solo per admin.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = Roles.ADMIN_ROLE)]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userManager.Users.Where(u => !u.IsDeleted).ToListAsync();
            var dtos = new List<UserDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var role = roles.FirstOrDefault() ?? "Nessun ruolo assegnato";

                dtos.Add(_mapper.MapEntityToDto(user, role));
            }

            return Ok(dtos);
        }


        /// <summary>
        /// Mostra il dettaglio di un singolo utente tramite il suo ID. Solo per admin.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id}")]
        [Authorize(Roles = Roles.ADMIN_ROLE)]
        public async Task<IActionResult> GetUserById([FromRoute] string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound(new { Message = $"Utente con ID '{id}' non trovato." });

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "Nessun ruolo assegnato";
            var dto = _mapper.MapEntityToDto(user, role);

            return Ok(dto);
        }


        /// <summary>
        /// Creazione manuale di un nuovo utente. Solo per admin.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = Roles.ADMIN_ROLE)]
        public async Task<IActionResult> CreateUser(UserCreateDto dto)
        {
            var roleExists = await _roleManager.RoleExistsAsync(dto.Role);
            if (!roleExists)
            {
                return BadRequest(new { Message = $"Il ruolo '{dto.Role}' non è valido nel sistema." });
            }

            var existingEmail = await _userManager.FindByEmailAsync(dto.Email);
            if (existingEmail != null)
            {
                return BadRequest(new { Message = "Questa email è già associata a un utente registrato." });
            }

            var newUser = new User
            {
                UserName = dto.Email,
                Email = dto.Email,
                Name = dto.Name,
                Surname = dto.Surname,
                IsDeleted = false
            };

            var result = await _userManager.CreateAsync(newUser, dto.Password);
            if (!result.Succeeded)
            {
                return UnprocessableEntity(result.Errors);
            }

            await _userManager.AddToRoleAsync(newUser, dto.Role);
            var resultDto = _mapper.MapEntityToDto(newUser, dto.Role);

            return CreatedAtAction(nameof(GetUserById), new { id = newUser.Id }, resultDto);
        }


        /// <summary>
        /// Modifica dei dati di un utente esistente. Solo per admin.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{id}")]
        [Authorize(Roles = Roles.ADMIN_ROLE)]
        public async Task<IActionResult> UpdateUser([FromRoute] string id, UserUpdateDto dto)
        {
            var user = await _userManager.FindByIdAsync(id);
            
            if (user == null || user.IsDeleted)
                return NotFound(new { Message = $"Utente con ID '{id}' non trovato." });

            if ((!string.IsNullOrEmpty(dto.Email)) && user.Email != dto.Email)
            {
                var existingEmail = await _userManager.FindByEmailAsync(dto.Email);
                if (existingEmail != null)
                {
                    return BadRequest(new { Message = "La nuova email specificata è già in uso da un altro utente." });
                }

                user.UserName = dto.Email;
                user.Email = dto.Email;
            }

            if (!string.IsNullOrEmpty(dto.Name)) user.Name = dto.Name;
            if (!string.IsNullOrEmpty(dto.Surname)) user.Surname = dto.Surname;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(new { Message = "Errore nell'aggiornamento dell'utente." });
            }

            return NoContent();
        }


        /// <summary>
        /// Sospensione di un utente (soft delete). Solo per admin.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPatch]
        [Route("{id}/suspend")]
        [Authorize(Roles = Roles.ADMIN_ROLE)]
        public async Task<IActionResult> SuspendUser([FromRoute] string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound(new { Message = $"Utente con ID '{id}' non trovato." });

            if (user.IsDeleted)
            {
                return BadRequest(new { Message = "L'utente è già sospeso." });
            }

            user.IsDeleted = true;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(new { Message = "Errore nella sospensione dell'utente." });
            }

            return NoContent();
        }
    }
}
