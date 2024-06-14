using API.DTOs;
using AutoMapper;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controller;

[ApiVersion("1.0", Deprecated = true)]                 // Ignora la version.
[ApiVersion("1.1")]                                    // Indicacion del versionado. 
[Authorize(Roles = "Administrador")]                        
// [Route("api/v{v:apiVersion}/datosproductos")]       // Configuracion de la ruta del controlador para el versionado por url.
public class DatosProductosController : BaseApiController
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public DatosProductosController(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    [HttpGet]                // Si no se especifica se toma la version por defecto.
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<ProductoListDTO>>> Get()
    {
        var productos = await _unitOfWork.Productos.GetAllAsync();

        return _mapper.Map<List<ProductoListDTO>>(productos);
    }


    [HttpGet]
    [MapToApiVersion("1.1")]   // Se especifica la version que se quiere utilizar.
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<ProductoDTO>>> GetProductoDesglosados()
    {
        var productos = await _unitOfWork.Productos.GetAllAsync();

        return _mapper.Map<List<ProductoDTO>>(productos);
    }


    //Ejemplos de versionados en postman.
    //http://localhost:5000/api/v0.8/datosproductos       Por url
    //http://localhost:5000/api/datosproductos?v=0.8      Por QueryString
    // Por Header o Encabezado se configura en el apartado Headers estableciendo un nuevo encabezado y especificando la version.
}