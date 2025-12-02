using ApiBizly.Models;
using ApiBizly.Services;
using HotChocolate;

namespace ApiBizly.GraphQL;

[ExtendObjectType(Name = "Mutation")]
public class ClienteMutations
{
    public async Task<Cliente> CrearCliente(
        [Service] ClienteService service,
        [Service] EmpresaService empresaService,
        [Service] SucursalService sucursalService,
        Cliente input)
    {
        // VALIDAR EmpresaId
        var empresa = await empresaService.GetByIdAsync(input.EmpresaId);
        if (empresa is null)
            throw new GraphQLException("La empresa especificada no existe.");

        // VALIDAR SucursalId (si se proporciona)
        if (!string.IsNullOrEmpty(input.SucursalId))
        {
            var sucursal = await sucursalService.GetByIdAsync(input.SucursalId);
            if (sucursal is null)
                throw new GraphQLException("La sucursal especificada no existe.");
        }

        input.Id = null;
        input.CreatedAt = DateTime.UtcNow;

        await service.CreateAsync(input);
        return input;
    }

    public async Task<bool> ActualizarCliente(
        [Service] ClienteService service,
        [Service] EmpresaService empresaService,
        [Service] SucursalService sucursalService,
        string id,
        Cliente input)
    {
        var existing = await service.GetByIdAsync(id);
        if (existing is null) return false;

        // VALIDAR EmpresaId
        var empresa = await empresaService.GetByIdAsync(input.EmpresaId);
        if (empresa is null)
            throw new GraphQLException("La empresa especificada no existe.");

        // VALIDAR SucursalId (si se proporciona)
        if (!string.IsNullOrEmpty(input.SucursalId))
        {
            var sucursal = await sucursalService.GetByIdAsync(input.SucursalId);
            if (sucursal is null)
                throw new GraphQLException("La sucursal especificada no existe.");
        }

        input.Id = id;

        await service.UpdateAsync(id, input);
        return true;
    }

    public async Task<bool> EliminarCliente(
        [Service] ClienteService service,
        string id)
    {
        var existing = await service.GetByIdAsync(id);
        if (existing is null) return false;

        await service.DeleteAsync(id);
        return true;
    }
}
