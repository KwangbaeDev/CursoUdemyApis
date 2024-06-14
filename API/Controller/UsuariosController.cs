using API.DTOs;
using API.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controller;

public class UsuariosController : BaseApiController
{
    private readonly IUserService _userService;

    public UsuariosController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("register")]
    public async Task<ActionResult> RegisterAsync(RegisterDTO model)
    {
        var result = await _userService.RegisterAsync(model);
        return Ok(result);
    }

    [HttpPost("token")]
    public async Task<IActionResult> GetTokenAsync(LoginDTO model)
    {
        var result = await _userService.GetTokenAsync(model);
        SetRefreshTokenInCookie(result.RefreshToken);
        return Ok(result);
    }


    [HttpPost("addrole")]
    public async Task<IActionResult> AddRoleAsync(AddRoleDTO model)
    {
        var result = await _userService.AddRoleAsync(model);
        return Ok(result);
    }


    private void SetRefreshTokenInCookie(string refreshToken)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = DateTime.UtcNow.AddDays(10)
        };
        Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
    }
}