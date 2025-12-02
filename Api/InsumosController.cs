using ApiBizly.Models;
using ApiBizly.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiBizly.Api;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "EMPRENDEDOR")]
public class InsumosController : ControllerBase
{
    private readonly InsumoService _service;
    private readonly EmpresaService _empresaService;
    private readonly SucursalService _sucursalService;
    private readonly CategoriaService _categoriaService;

    public InsumosController(
        InsumoService service,
        EmpresaService empresaService,
        SucursalService sucursalService,
        CategoriaService categoriaService)
    {
        _service = service;
        _empresaService = empresaService;
        _sucursalService = sucursalService;
        _categoriaService = categoriaService;
    }

    [HttpGet]
    public async Task<ActionResult<List<Insumo>>> Get()
    {
        var data = await _service.GetAllAsync();
        return Ok(data);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Insumo>> GetById(string id)
    {
        var entity = await _service.GetByIdAsync(id);
        if (entity is null) return NotFound();
        return Ok(entity);
    }

    [HttpPost]
    public async Task<ActionResult<Insumo>> Post([FromBody] Insumo entity)
    {
        // VALIDAR EmpresaId
        var empresa = await _empresaService.GetByIdAsync(entity.EmpresaId);
        if (empresa is null)
            return BadRequest("La empresa especificada no existe.");

        // VALIDAR SucursalId
        var sucursal = await _sucursalService.GetByIdAsync(entity.SucursalId);
        if (sucursal is null)
            return BadRequest("La sucursal especificada no existe.");

        // VALIDAR CategoriaId (si se proporciona)
        if (!string.IsNullOrEmpty(entity.CategoriaId))
        {
            var categoria = await _categoriaService.GetByIdAsync(entity.CategoriaId);
            if (categoria is null)
                return BadRequest("La categoría especificada no existe.");
        }

        entity.PrecioTotal = entity.PrecioUnitario * entity.Cantidad;

        await _service.CreateAsync(entity);

        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(string id, [FromBody] Insumo entity)
    {
        var existing = await _service.GetByIdAsync(id);
        if (existing is null) return NotFound();

        // VALIDAR EmpresaId
        var empresa = await _empresaService.GetByIdAsync(entity.EmpresaId);
        if (empresa is null)
            return BadRequest("La empresa especificada no existe.");

        // VALIDAR SucursalId
        var sucursal = await _sucursalService.GetByIdAsync(entity.SucursalId);
        if (sucursal is null)
            return BadRequest("La sucursal especificada no existe.");

        // VALIDAR CategoriaId (si se proporciona)
        if (!string.IsNullOrEmpty(entity.CategoriaId))
        {
            var categoria = await _categoriaService.GetByIdAsync(entity.CategoriaId);
            if (categoria is null)
                return BadRequest("La categoría especificada no existe.");
        }

        entity.PrecioTotal = entity.PrecioUnitario * entity.Cantidad;

        entity.Id = id;

        await _service.UpdateAsync(id, entity);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var existing = await _service.GetByIdAsync(id);
        if (existing is null) return NotFound();

        await _service.DeleteAsync(id);
        return NoContent();
    }
}
