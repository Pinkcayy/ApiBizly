using ApiBizly.Models;
using ApiBizly.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiBizly.Api;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "EMPRENDEDOR,TRABAJADOR")]
public class DetalleVentasController : ControllerBase
{
    private readonly DetalleVentaService _service;
    private readonly VentaService _ventaService;
    private readonly ProductoVentaService _productoVentaService;

    public DetalleVentasController(
        DetalleVentaService service,
        VentaService ventaService,
        ProductoVentaService productoVentaService)
    {
        _service = service;
        _ventaService = ventaService;
        _productoVentaService = productoVentaService;
    }

    [HttpGet]
    public async Task<ActionResult<List<DetalleVenta>>> Get()
    {
        var data = await _service.GetAllAsync();
        return Ok(data);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DetalleVenta>> GetById(string id)
    {
        var entity = await _service.GetByIdAsync(id);
        if (entity is null) return NotFound();
        return Ok(entity);
    }

    [HttpGet("venta/{ventaId}")]
    public async Task<ActionResult<List<DetalleVenta>>> GetByVentaId(string ventaId)
    {
        var data = await _service.GetByVentaIdAsync(ventaId);
        return Ok(data);
    }

    [HttpPost]
    public async Task<ActionResult<DetalleVenta>> Post([FromBody] DetalleVenta entity)
    {
        // VALIDAR VentaId
        var venta = await _ventaService.GetByIdAsync(entity.VentaId);
        if (venta is null)
            return BadRequest("La venta especificada no existe.");

        // VALIDAR ProductoVentaId
        var productoVenta = await _productoVentaService.GetByIdAsync(entity.ProductoVentaId);
        if (productoVenta is null)
            return BadRequest("El producto de venta especificado no existe.");

        // Calcular Subtotal si no se proporciona
        if (entity.Subtotal <= 0)
        {
            entity.Subtotal = entity.PrecioUnitario * entity.Cantidad;
        }

        await _service.CreateAsync(entity);
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(string id, [FromBody] DetalleVenta entity)
    {
        var existing = await _service.GetByIdAsync(id);
        if (existing is null) return NotFound();

        // VALIDAR VentaId
        var venta = await _ventaService.GetByIdAsync(entity.VentaId);
        if (venta is null)
            return BadRequest("La venta especificada no existe.");

        // VALIDAR ProductoVentaId
        var productoVenta = await _productoVentaService.GetByIdAsync(entity.ProductoVentaId);
        if (productoVenta is null)
            return BadRequest("El producto de venta especificado no existe.");

        // Calcular Subtotal si no se proporciona
        if (entity.Subtotal <= 0)
        {
            entity.Subtotal = entity.PrecioUnitario * entity.Cantidad;
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

