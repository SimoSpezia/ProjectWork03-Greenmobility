using GreenMobility_be.Data;
using GreenMobility_be.Dto;

namespace GreenMobility_be.Mapper
{
    public class UserMapper
    {
        public UserDto MapEntityToDto(User entity, string role)
        {
            UserDto dto = new UserDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Surname = entity.Surname,
                Email = entity.Email ?? string.Empty,
                Role = role,
                IsDeleted = entity.IsDeleted
            };

            return dto;
        }
    }
}
