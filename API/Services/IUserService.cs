using API.DTOs;

namespace API.Services;

public interface IUserService
{
    Task<string> RegisterAsync(RegisterDTO model);
    Task<DatosUsuarioDTO> GetTokenAsync(LoginDTO model);
    Task<string> AddRoleAsync(AddRoleDTO model);
}