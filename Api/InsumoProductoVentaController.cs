using ApiBizly.Models;
using ApiBizly.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiBizly.Api;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "EMPRENDEDOR")]
public class InsumoProductoVentaController : ControllerBase
{
    private readonly InsumoProductoVentaService _service;
    private readonly ProductoVentaService _productoVentaService;
    private readonly InsumoService _insumoService;

    public InsumoProductoVentaController(
        InsumoProductoVentaService service,
        ProductoVentaService productoVentaService,
        InsumoService insumoService)
    {
        _service = service;
        _productoVentaService = productoVentaService;
        _insumoService = insumoService;
    }

    [HttpGet]
    public async Task<ActionResult<List<InsumoProductoVenta>>> Get()
    {
        var data = await _service.GetAllAsync();
        return Ok(data);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<InsumoProductoVenta>> GetById(string id)
    {
        var entity = await _service.GetByIdAsync(id);
        if (entity is null) return NotFound();
        return Ok(entity);
    }

    [HttpGet("producto/{productoVentaId}")]
    public async Task<ActionResult<List<InsumoProductoVenta>>> GetByProductoVentaId(string productoVentaId)
    {
        var data = await _service.GetByProductoVentaIdAsync(productoVentaId);
        return Ok(data);
    }

    [HttpPost]
    public async Task<ActionResult<InsumoProductoVenta>> Post([FromBody] InsumoProductoVenta entity)
    {
        // VALIDAR ProductoVentaId
        var productoVenta = await _productoVentaService.GetByIdAsync(entity.ProductoVentaId);
        if (productoVenta is null)
            return BadRequest("El producto de venta especificado no existe.");

        // VALIDAR InsumoId
        var insumo = await _insumoService.GetByIdAsync(entity.InsumoId);
        if (insumo is null)
            return BadRequest("El insumo especificado no existe.");

        await _service.CreateAsync(entity);
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(string id, [FromBody] InsumoProductoVenta entity)
    {
        var existing = await _service.GetByIdAsync(id);
        if (existing is null) return NotFound();

        // VALIDAR ProductoVentaId
        var productoVenta = await _productoVentaService.GetByIdAsync(entity.ProductoVentaId);
        if (productoVenta is null)
            return BadRequest("El producto de venta especificado no existe.");

        // VALIDAR InsumoId
        var insumo = await _insumoService.GetByIdAsync(entity.InsumoId);
        if (insumo is null)
            return BadRequest("El insumo especificado no existe.");

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

