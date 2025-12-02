using ApiBizly.Models;
using ApiBizly.Services;
using Microsoft.AspNetCore.Mvc;

namespace ApiBizly.Api;

[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly UsuarioService _service;
    private readonly EmpresaService _empresaService;
    private readonly SucursalService _sucursalService;
    private readonly TrabajadorService _trabajadorService;

    public UsuariosController(
        UsuarioService service,
        EmpresaService empresaService,
        SucursalService sucursalService,
        TrabajadorService trabajadorService)
    {
        _service = service;
        _empresaService = empresaService;
        _sucursalService = sucursalService;
        _trabajadorService = trabajadorService;
    }

    [HttpGet]
    public async Task<ActionResult<List<Usuario>>> Get()
    {
        var data = await _service.GetAllAsync();
        return Ok(data);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Usuario>> GetById(string id)
    {
        var entity = await _service.GetByIdAsync(id);
        if (entity is null) return NotFound();
        return Ok(entity);
    }

    [HttpPost]
    public async Task<ActionResult<Usuario>> Post([FromBody] Usuario entity)
    {
        // VALIDAR EmpresaId
        var empresa = await _empresaService.GetByIdAsync(entity.EmpresaId);
        if (empresa is null)
            return BadRequest("La empresa especificada no existe.");

        // VALIDAR SucursalId (si se proporciona)
        if (!string.IsNullOrEmpty(entity.SucursalId))
        {
            var sucursal = await _sucursalService.GetByIdAsync(entity.SucursalId);
            if (sucursal is null)
                return BadRequest("La sucursal especificada no existe.");
        }

        // VALIDAR TrabajadorId (si se proporciona)
        if (!string.IsNullOrEmpty(entity.TrabajadorId))
        {
            var trabajador = await _trabajadorService.GetByIdAsync(entity.TrabajadorId);
            if (trabajador is null)
                return BadRequest("El trabajador especificado no existe.");
        }

        await _service.CreateAsync(entity);
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(string id, [FromBody] Usuario entity)
    {
        var existing = await _service.GetByIdAsync(id);
        if (existing is null) return NotFound();

        // VALIDAR EmpresaId
        var empresa = await _empresaService.GetByIdAsync(entity.EmpresaId);
        if (empresa is null)
            return BadRequest("La empresa especificada no existe.");

        // VALIDAR SucursalId (si se proporciona)
        if (!string.IsNullOrEmpty(entity.SucursalId))
        {
            var sucursal = await _sucursalService.GetByIdAsync(entity.SucursalId);
            if (sucursal is null)
                return BadRequest("La sucursal especificada no existe.");
        }

        // VALIDAR TrabajadorId (si se proporciona)
        if (!string.IsNullOrEmpty(entity.TrabajadorId))
        {
            var trabajador = await _trabajadorService.GetByIdAsync(entity.TrabajadorId);
            if (trabajador is null)
                return BadRequest("El trabajador especificado no existe.");
        }

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
