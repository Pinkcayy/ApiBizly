using ApiBizly.Models;
using ApiBizly.Services;
using Microsoft.AspNetCore.Mvc;

namespace ApiBizly.Api;

[ApiController]
[Route("api/[controller]")]
public class VentasController : ControllerBase
{
    private readonly VentaService _service;
    private readonly EmpresaService _empresaService;
    private readonly SucursalService _sucursalService;
    private readonly UsuarioService _usuarioService;
    private readonly ClienteService _clienteService;

    public VentasController(
        VentaService service,
        EmpresaService empresaService,
        SucursalService sucursalService,
        UsuarioService usuarioService,
        ClienteService clienteService)
    {
        _service = service;
        _empresaService = empresaService;
        _sucursalService = sucursalService;
        _usuarioService = usuarioService;
        _clienteService = clienteService;
    }

    // GET api/Ventas
    [HttpGet]
    public async Task<ActionResult<List<Venta>>> Get()
    {
        var data = await _service.GetAllAsync();
        return Ok(data);
    }

    // GET api/Ventas/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<Venta>> GetById(string id)
    {
        var entity = await _service.GetByIdAsync(id);
        if (entity is null) return NotFound();
        return Ok(entity);
    }

    // POST api/Ventas
    [HttpPost]
    public async Task<ActionResult<Venta>> Post([FromBody] Venta entity)
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

        // VALIDAR ClienteId (si se proporciona)
        if (!string.IsNullOrEmpty(entity.ClienteId))
        {
            var cliente = await _clienteService.GetByIdAsync(entity.ClienteId);
            if (cliente is null)
                return BadRequest("El cliente especificado no existe.");
        }

        await _service.CreateAsync(entity);
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
    }

    // PUT api/Ventas/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Put(string id, [FromBody] Venta entity)
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

        // VALIDAR ClienteId (si se proporciona)
        if (!string.IsNullOrEmpty(entity.ClienteId))
        {
            var cliente = await _clienteService.GetByIdAsync(entity.ClienteId);
            if (cliente is null)
                return BadRequest("El cliente especificado no existe.");
        }

        entity.Id = id;
        await _service.UpdateAsync(id, entity);
        return NoContent();
    }

    // DELETE api/Ventas/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var existing = await _service.GetByIdAsync(id);
        if (existing is null) return NotFound();

        await _service.DeleteAsync(id);
        return NoContent();
    }
}
