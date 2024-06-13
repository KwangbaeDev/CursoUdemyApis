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
}