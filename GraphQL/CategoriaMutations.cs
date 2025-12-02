using ApiBizly.Models;
using ApiBizly.Services;
using HotChocolate;

namespace ApiBizly.GraphQL;

[ExtendObjectType(Name = "Mutation")]
public class CategoriaMutations
{
    public async Task<Categoria> CrearCategoria(
        [Service] CategoriaService service,
        [Service] EmpresaService empresaService,
        Categoria input)
    {
        // VALIDAR EmpresaId
        var empresa = await empresaService.GetByIdAsync(input.EmpresaId);
        if (empresa is null)
            throw new GraphQLException("La empresa especificada no existe.");

        input.Id = null;
        input.CreatedAt = DateTime.UtcNow;

        await service.CreateAsync(input);
        return input;
    }

    public async Task<bool> ActualizarCategoria(
        [Service] CategoriaService service,
        [Service] EmpresaService empresaService,
        string id,
        Categoria input)
    {
        var existing = await service.GetByIdAsync(id);
        if (existing is null) return false;

        // VALIDAR EmpresaId
        var empresa = await empresaService.GetByIdAsync(input.EmpresaId);
        if (empresa is null)
            throw new GraphQLException("La empresa especificada no existe.");

        input.Id = id;

        await service.UpdateAsync(id, input);
        return true;
    }

    public async Task<bool> EliminarCategoria(
        [Service] CategoriaService service,
        string id)
    {
        var existing = await service.GetByIdAsync(id);
        if (existing is null) return false;

        await service.DeleteAsync(id);
        return true;
    }
}
