using ApiBizly.Models;
using ApiBizly.Services;
using HotChocolate;

namespace ApiBizly.GraphQL;

[ExtendObjectType("Mutation")]
public class UsuarioMutations
{
    public async Task<Usuario> CrearUsuario(
        [Service] UsuarioService service,
        [Service] EmpresaService empresaService,
        [Service] SucursalService sucursalService,
        [Service] TrabajadorService trabajadorService,
        Usuario input)
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

        // VALIDAR TrabajadorId (si se proporciona)
        if (!string.IsNullOrEmpty(input.TrabajadorId))
        {
            var trabajador = await trabajadorService.GetByIdAsync(input.TrabajadorId);
            if (trabajador is null)
                throw new GraphQLException("El trabajador especificado no existe.");
        }

        input.Id = null;
        input.CreatedAt = DateTime.UtcNow;
        input.UpdatedAt = DateTime.UtcNow;

        await service.CreateAsync(input);
        return input;
    }

    public async Task<bool> ActualizarUsuario(
        [Service] UsuarioService service,
        [Service] EmpresaService empresaService,
        [Service] SucursalService sucursalService,
        [Service] TrabajadorService trabajadorService,
        string id,
        Usuario input)
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

        // VALIDAR TrabajadorId (si se proporciona)
        if (!string.IsNullOrEmpty(input.TrabajadorId))
        {
            var trabajador = await trabajadorService.GetByIdAsync(input.TrabajadorId);
            if (trabajador is null)
                throw new GraphQLException("El trabajador especificado no existe.");
        }

        input.Id = id;
        input.UpdatedAt = DateTime.UtcNow;

        await service.UpdateAsync(id, input);
        return true;
    }

    public async Task<bool> EliminarUsuario(
        [Service] UsuarioService service,
        string id)
    {
        var existing = await service.GetByIdAsync(id);
        if (existing is null) return false;

        await service.DeleteAsync(id);
        return true;
    }
}
