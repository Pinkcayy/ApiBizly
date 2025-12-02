using ApiBizly.Models;
using ApiBizly.Services;
using HotChocolate;

namespace ApiBizly.GraphQL;

[ExtendObjectType("Query")]
public class CategoriaQueries
{
    public Task<List<Categoria>> GetCategorias(
        [Service] CategoriaService service) =>
        service.GetAllAsync();

    public Task<Categoria?> GetCategoriaById(
        [Service] CategoriaService service,
        string id) =>
        service.GetByIdAsync(id);
}
