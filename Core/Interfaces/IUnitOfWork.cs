using Core.Entities;

namespace Core.Interfaces;

public interface IUnitOfWork
{
    IProductoRepository Productos { get; }
    IMarcaRepository Marcas { get; }
    ICategoriaRepository Categorias { get; }
    Task<int> SaveAsync();
}