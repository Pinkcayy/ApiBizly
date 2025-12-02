using ApiBizly.Models;
using ApiBizly.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiBizly.Api;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "EMPRENDEDOR")]
public class SucursalesController : ControllerBase
{
    private readonly SucursalService _service;
    private readonly EmpresaService _empresaService;

    public SucursalesController(
        SucursalService service,
        EmpresaService empresaService)
    {
        _service = service;
        _empresaService = empresaService;
    }

    [HttpGet]
    public async Task<ActionResult<List<Sucursal>>> Get()
    {
        var data = await _service.GetAllAsync();
        return Ok(data);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Sucursal>> GetById(string id)
    {
        var entity = await _service.GetByIdAsync(id);
        if (entity is null) return NotFound();
        return Ok(entity);
    }

    [HttpPost]
    public async Task<ActionResult<Sucursal>> Post([FromBody] Sucursal entity)
    {
        // VALIDAR EmpresaId
        var empresa = await _empresaService.GetByIdAsync(entity.EmpresaId);
        if (empresa is null)
            return BadRequest("La empresa especificada no existe.");

        await _service.CreateAsync(entity);
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(string id, [FromBody] Sucursal entity)
    {
        var existing = await _service.GetByIdAsync(id);
        if (existing is null) return NotFound();

        // VALIDAR EmpresaId
        var empresa = await _empresaService.GetByIdAsync(entity.EmpresaId);
        if (empresa is null)
            return BadRequest("La empresa especificada no existe.");

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

