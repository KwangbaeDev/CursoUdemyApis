using API.DTOs;
using API.Helpers;
using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace API.Services;

public class UserService : IUserService
{
    private readonly JWT _jwt;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher<Usuario> _passwordHasher;

    public UserService(IUnitOfWork unitOfWork, IOptions<JWT> jwt, IPasswordHasher<Usuario> passwordHasher)
    {
        _jwt = jwt.Value;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
    }


    public async Task<string> RegisterAsync(RegisterDTO registerDTO)
    {
        var usuario = new Usuario
        {
            Nombres = registerDTO.Nombres,
            ApellidoPaterno = registerDTO.ApellidoPaterno,
            ApellidoMaterno = registerDTO.ApellidoMaterno,
            Email = registerDTO.Email,
            Username = registerDTO.Username
        };

        usuario.Password = _passwordHasher.HashPassword(usuario, registerDTO.Password);

        var usuarioExiste = _unitOfWork.Usuarios
                                    .Find(u => u.Username.ToLower() == registerDTO.Username.ToLower())
                                    .FirstOrDefault();
        
        if (usuarioExiste == null)
        {
            var rolPredeterminado = _unitOfWork.Roles
                                            .Find(r => r.Nombre == Autorizacion.rol_predeterminado.ToString())
                                            .First();

            try
            {
                usuario.Roles.Add(rolPredeterminado);
                _unitOfWork.Usuarios.Add(usuario);
                await _unitOfWork.SaveAsync();

                return $"El usuario {registerDTO.Username} ha sido registrado exitosamente";
            }
            catch (Exception ex)
            {
                var message = ex.Message;
                return $"Error: {message}";
            }
        }
        else
        {
            return $"El usuario con {registerDTO.Username} ya se encuentra registrado";
        }
    }


    public async Task<DatosUsuarioDTO> GetTokenAsync(LoginDTO model)
    {
        DatosUsuarioDTO datosUsuarioDTO= new DatosUsuarioDTO();
        var usuario = await _unitOfWork.Usuarios
                                    .GetByUsernameAsync(model.Username);

        if (usuario == null)
        {
            datosUsuarioDTO.EstaAutenticado = false;
            datosUsuarioDTO.Mensaje = $"No existe ningun usuario con el username {model.Username}.";
            return datosUsuarioDTO;
        }

        var resultado = _passwordHasher.VerifyHashedPassword(usuario, usuario.Password, model.Password);

        if (resultado == PasswordVerificationResult.Success)
        {
            // Falta escribir el codigo.
        }
        datosUsuarioDTO.EstaAutenticado = false;
        datosUsuarioDTO.Mensaje = $"Credenciales incorrectas para el usuario {usuario.Username}.";
        return datosUsuarioDTO;
    }
}