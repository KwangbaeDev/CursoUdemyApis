using Core.Interfaces;
using Infrastructure.Data;
using Infrastructure.Repositories;

namespace Infrastructure.UnitOfWork;

public class UnitOfWork : IUnitOfWork , IDisposable
{
    private readonly TiendaContext _context;
    private IProductoRepository _productos;
    private IMarcaRepository _marcas;
    private ICategoriaRepository _categorias;

    public UnitOfWork(TiendaContext context)
    {
        _context = context;
    }
    public IProductoRepository Productos
    {
        get
        {
            if (_productos == null)
            {
                _productos = new ProductoRepository(_context);
            }
            return _productos;
        }
    }

    public IMarcaRepository Marcas
    {
        get
        {
            if (_marcas == null)
            {
                _marcas = new MarcaRepository(_context);
            }
            return _marcas;
        }
    }

    public ICategoriaRepository Categorias
    {
        get
        {
            if (_categorias == null)
            {
                _categorias = new CategoriaRepository(_context);
            }
            return _categorias;
        }
    }

    public int Save()
    {
        return _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}