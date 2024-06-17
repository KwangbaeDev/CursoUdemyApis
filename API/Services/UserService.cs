using System.IdentityModel.Tokens.Jwt;
using System.IO.Compression;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using API.DTOs;
using API.Helpers;
using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

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
            datosUsuarioDTO.EstaAutenticado = true;
            JwtSecurityToken jwtSecurityToken = CreateJwtToken(usuario);
            datosUsuarioDTO.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            datosUsuarioDTO.Email =  usuario.Email;
            datosUsuarioDTO.UserName = usuario.Username;
            datosUsuarioDTO.Roles = usuario.Roles
                                            .Select(r => r.Nombre)
                                            .ToList();

            if (usuario.RefreshTokens.Any(rt => rt.IsActive))
            {
                var activeRefreshToken = usuario.RefreshTokens.Where(rt => rt.IsActive == true).FirstOrDefault();
                datosUsuarioDTO.RefreshToken = activeRefreshToken.Token;
                datosUsuarioDTO.RefreshTokenExpiration = activeRefreshToken.Expires;
            }
            else
            {
                var refreshToken = CreateRefreshToken();
                datosUsuarioDTO.RefreshToken = refreshToken.Token;
                datosUsuarioDTO.RefreshTokenExpiration = refreshToken.Expires;
                usuario.RefreshTokens.Add(refreshToken);
                _unitOfWork.Usuarios.Update(usuario);
                await _unitOfWork.SaveAsync();
            }     
            return datosUsuarioDTO;
        }
        datosUsuarioDTO.EstaAutenticado = false;
        datosUsuarioDTO.Mensaje = $"Credenciales incorrectas para el usuario {usuario.Username}.";
        return datosUsuarioDTO;
    }

    public async Task<string> AddRoleAsync(AddRoleDTO model)
    {
        var usuario = await _unitOfWork.Usuarios
                                    .GetByUsernameAsync(model.Username);
        if (usuario == null)  return $"No existe algun usuario registrado con la cuenta {model.Username}.";

        var resultado = _passwordHasher.VerifyHashedPassword(usuario, usuario.Password, model.Password);

        if (resultado == PasswordVerificationResult.Success)
        {
            var rolExiste = _unitOfWork.Roles
                                        .Find(r => r.Nombre.ToLower() == model.Role.ToLower())
                                        .FirstOrDefault();
            if (rolExiste != null)
            {
                var usuarioTieneRol = usuario.Roles
                                        .Any(u => u.Id == rolExiste.Id);
                if (usuarioTieneRol == false)
                {
                    usuario.Roles.Add(rolExiste);
                    _unitOfWork.Usuarios.Update(usuario);
                    await _unitOfWork.SaveAsync();
                }
                return $"Rol {model.Role} agregado a la cuenta {model.Username} de forma exitosa";
            }
            return $"Rol {model.Role} no encontrado";
        }
        return $"Credenciales incorrectas para el usuario {usuario.Username}";
    }


    public async Task<DatosUsuarioDTO> RefreshTokenAsync(string refreshToken)
    {
        var datosUsuarioDTO = new DatosUsuarioDTO();

        var usuario = await _unitOfWork.Usuarios
                                    .GetByRefreshTokenAsync(refreshToken);

        if (usuario == null)
        {
            datosUsuarioDTO.EstaAutenticado = false;
            datosUsuarioDTO.Mensaje = $"El token no pertenece a ningun usuario.";
            return datosUsuarioDTO;
        }

        var refreshTokenBd = usuario.RefreshTokens.Single(rt => rt.Token == refreshToken);

        if (!refreshTokenBd.IsActive)
        {
            datosUsuarioDTO.EstaAutenticado = false;
            datosUsuarioDTO.Mensaje = $"El token no esta activo.";
            return datosUsuarioDTO;
        }

        //Revocamos el Refresh Token actual y
        refreshTokenBd.Revoked = DateTime.UtcNow;
        //generamos un nuevo Refresh Token y lo guardamos en la Base de Datos
        var newRefreshToken = CreateRefreshToken();
        usuario.RefreshTokens.Add(newRefreshToken);
        _unitOfWork.Usuarios.Update(usuario);
        await _unitOfWork.SaveAsync();
        //Generamos un nuevo Json Web Token
        datosUsuarioDTO.EstaAutenticado = true;
        JwtSecurityToken jwtSecurityToken = CreateJwtToken(usuario);
        datosUsuarioDTO.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
        datosUsuarioDTO.Email = usuario.Email;
        datosUsuarioDTO.UserName = usuario.Username;
        datosUsuarioDTO.Roles = usuario.Roles
                                        .Select(r => r.Nombre)
                                        .ToList();
        datosUsuarioDTO.RefreshToken = newRefreshToken.Token;
        datosUsuarioDTO.RefreshTokenExpiration = newRefreshToken.Expires;
        return datosUsuarioDTO;
    }

    private RefreshToken CreateRefreshToken()
    {
        var randomNumber = new byte[32];
        using (var generator = RandomNumberGenerator.Create())
        {
            generator.GetBytes(randomNumber);
            return new RefreshToken
            {
                Token = Convert.ToBase64String(randomNumber),
                Expires = DateTime.UtcNow.AddDays(10),
                Created = DateTime.UtcNow
            };
        }
    }

    private JwtSecurityToken CreateJwtToken(Usuario usuario)
    {
        var roles = usuario.Roles;
        var roleClaims = new List<Claim>();
        foreach (var role in roles)
        {
            roleClaims.Add(new Claim("roles", role.Nombre));
        }
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
            new Claim("uid", usuario.Id.ToString())
        }
        .Union(roleClaims);
        // string tokenOriginal = _jwt.Key;
        // string tokenInverso = new string(tokenOriginal.Reverse().ToArray());
        // string tokenFinal = tokenOriginal + tokenInverso;
        var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
        var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);
        var jwtSecurityToken =  new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwt.DurationInMinutes),
            signingCredentials: signingCredentials
            );
        return jwtSecurityToken;
    }
}