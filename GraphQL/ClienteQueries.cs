using ApiBizly.Models;
using ApiBizly.Services;
using HotChocolate;

namespace ApiBizly.GraphQL;

[ExtendObjectType("Query")]
public class ClienteQueries
{
    public Task<List<Cliente>> GetClientes(
        [Service] ClienteService service)
        => service.GetAllAsync();

    public Task<Cliente?> GetClienteById(
        [Service] ClienteService service,
        string id)
        => service.GetByIdAsync(id);
}
