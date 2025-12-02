using ApiBizly.Models;
using ApiBizly.Services;
using HotChocolate;

namespace ApiBizly.GraphQL;

[ExtendObjectType("Query")]
public class UsuarioQueries
{
    public Task<List<Usuario>> GetUsuarios(
        [Service] UsuarioService service) =>
        service.GetAllAsync();

    public Task<Usuario?> GetUsuarioById(
        [Service] UsuarioService service,
        string id) =>
        service.GetByIdAsync(id);
}
