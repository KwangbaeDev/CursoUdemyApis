using Core.Entities;
using Core.Interfaces;
using Infrastructure.Data;

namespace Infrastructure.Repositories;

public class CategoriaRepository : GenericRepository<Categoria> , ICategoriaRepository
{
    public CategoriaRepository(TiendaContext context) : base(context)
    {
    }
}