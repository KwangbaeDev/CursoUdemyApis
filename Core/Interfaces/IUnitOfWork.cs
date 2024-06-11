namespace Core.Interfaces;

public interface IUnitOfWork
{
    IProductoRepository Productos { get; }
    IMarcaRepository Marcas { get; }
    ICategoriaRepository Categorias { get; }
    int Save();
}