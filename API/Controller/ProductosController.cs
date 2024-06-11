using Core.Entities;
using Core.Interfaces;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controller;

public class ProductosController : BaseApiController
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductosController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<Producto>>> Get()
    {
        var products = await _unitOfWork.Productos.GetAllAsync();
        return Ok(products);
    }


    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Get(int id)
    {
        var producto = await _unitOfWork.Productos.GetByIdAsync(id);
        return Ok(producto);
    }
}