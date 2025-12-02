using ApiBizly.Models;
using ApiBizly.Services;
using Microsoft.AspNetCore.Mvc;

namespace ApiBizly.Api;

[ApiController]
[Route("api/[controller]")]
public class RegistrosInventarioController : ControllerBase
{
    private readonly RegistroInventarioService _service;
    private readonly EmpresaService _empresaService;
    private readonly SucursalService _sucursalService;
    private readonly UsuarioService _usuarioService;
    private readonly InsumoService _insumoService;

    public RegistrosInventarioController(
        RegistroInventarioService service,
        EmpresaService empresaService,
        SucursalService sucursalService,
        UsuarioService usuarioService,
        InsumoService insumoService)
    {
        _service = service;
        _empresaService = empresaService;
        _sucursalService = sucursalService;
        _usuarioService = usuarioService;
        _insumoService = insumoService;
    }

    [HttpGet]
    public async Task<ActionResult<List<RegistroInventario>>> Get()
    {
        var data = await _service.GetAllAsync();
        return Ok(data);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RegistroInventario>> GetById(string id)
    {
        var entity = await _service.GetByIdAsync(id);
        if (entity is null) return NotFound();
        return Ok(entity);
    }

    // GET api/RegistrosInventario/por-insumo/{insumoId}
    [HttpGet("por-insumo/{insumoId}")]
    public async Task<ActionResult<List<RegistroInventario>>> GetByInsumoId(string insumoId)
    {
        var data = await _service.GetByInsumoIdAsync(insumoId);
        return Ok(data);
    }

    // POST api/RegistrosInventario
    [HttpPost]
    public async Task<ActionResult<RegistroInventario>> Post([FromBody] RegistroInventario entity)
    {
        // VALIDAR EmpresaId
        var empresa = await _empresaService.GetByIdAsync(entity.EmpresaId);
        if (empresa is null)
            return BadRequest("La empresa especificada no existe.");

        // VALIDAR SucursalId
        var sucursal = await _sucursalService.GetByIdAsync(entity.SucursalId);
        if (sucursal is null)
            return BadRequest("La sucursal especificada no existe.");

        // VALIDAR UsuarioId
        var usuario = await _usuarioService.GetByIdAsync(entity.UsuarioId);
        if (usuario is null)
            return BadRequest("El usuario especificado no existe.");

        // VALIDAR InsumoId
        var insumo = await _insumoService.GetByIdAsync(entity.InsumoId);
        if (insumo is null)
            return BadRequest("El insumo especificado no existe.");

        entity.CantidadAnterior = insumo.Cantidad;

        await _service.CreateAsync(entity);
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
    }

    // PUT api/RegistrosInventario/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Put(string id, [FromBody] RegistroInventario entity)
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

        // VALIDAR UsuarioId
        var usuario = await _usuarioService.GetByIdAsync(entity.UsuarioId);
        if (usuario is null)
            return BadRequest("El usuario especificado no existe.");

        // VALIDAR InsumoId
        var insumo = await _insumoService.GetByIdAsync(entity.InsumoId);
        if (insumo is null)
            return BadRequest("El insumo especificado no existe.");

        entity.Id = id;

        await _service.UpdateAsync(id, entity);
        return NoContent();
    }

    // DELETE api/RegistrosInventario/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var existing = await _service.GetByIdAsync(id);
        if (existing is null) return NotFound();

        await _service.DeleteAsync(id);
        return NoContent();
    }
}
