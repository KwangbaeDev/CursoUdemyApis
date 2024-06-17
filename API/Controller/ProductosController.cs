using API.DTOs;
using API.Helpers;
using API.Helpers.Errors;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controller;

[Authorize(Roles = "Administrador")]  
public class ProductosController : BaseApiController
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ProductosController(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Pager<ProductoListDTO>>> Get([FromQuery] Params productParms)
    {
        var resultado = await _unitOfWork.Productos
                                    .GetAllAsync(productParms.PageIndex, productParms.PageSize, productParms.Search);

        var listaProductosDTO = _mapper.Map<List<ProductoListDTO>>(resultado.registros);

        Response.Headers.Add("X-InLineCount", resultado.totalRegistros.ToString()); 

        return new Pager<ProductoListDTO>(listaProductosDTO, resultado.totalRegistros, productParms.PageIndex,
             productParms.PageSize, productParms.Search);
    }


    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductoDTO>> Get(int id)
    {
        var producto = await _unitOfWork.Productos.GetByIdAsync(id);
        if (producto == null) return NotFound(new ApiResponse(404, "El producto solicitado no existe."));

        return _mapper.Map<ProductoDTO>(producto);
    }


    //POST: api/Productos
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Producto>> Post(ProductoAddUpdateDTO productoDTO)
    {
        var producto = _mapper.Map<Producto>(productoDTO);
        
        _unitOfWork.Productos.Add(producto);
        await _unitOfWork.SaveAsync();
        if (producto == null) return BadRequest(new ApiResponse(400));

        productoDTO.Id = producto.Id;
        return CreatedAtAction(nameof(Post), new {id=productoDTO.Id}, productoDTO);
    }

    //PUT: api/Productos
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductoAddUpdateDTO>> Put(int id, [FromBody] ProductoAddUpdateDTO productoDTO)
    {
        if (productoDTO == null) return NotFound(new ApiResponse(404, "El producto solicitado no existe."));

        var productoBd = await _unitOfWork.Productos.GetByIdAsync(id);
        if (productoBd == null) return NotFound(new ApiResponse(404, "El producto solicitado no existe."));

        var producto = _mapper.Map<Producto>(productoDTO);

        _unitOfWork.Productos.Update(producto);
        await _unitOfWork.SaveAsync();

        return productoDTO;
    }


    //DELETE: api/Productos
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var producto = await _unitOfWork.Productos.GetByIdAsync(id);
        if (producto == null) return NotFound(new ApiResponse(404, "El producto solicitado no existe."));

        _unitOfWork.Productos.Remove(producto);
        await _unitOfWork.SaveAsync();

        return NoContent();
    }   
}