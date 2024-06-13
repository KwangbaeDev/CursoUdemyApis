using Core.Entities;

namespace Core.Interfaces;

public interface IUnitOfWork
{
    IProductoRepository Productos { get; }
    IMarcaRepository Marcas { get; }
    ICategoriaRepository Categorias { get; }
    IUsuarioRepository Usuarios { get; }
    IRolRepository Roles { get; }
    Task<int> SaveAsync();
}