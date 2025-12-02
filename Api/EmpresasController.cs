using ApiBizly.Models;
using ApiBizly.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiBizly.Api;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "EMPRENDEDOR")]
public class EmpresasController : ControllerBase
{
    private readonly EmpresaService _service;

    public EmpresasController(EmpresaService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<Empresa>>> Get()
    {
        var data = await _service.GetAllAsync();
        return Ok(data);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Empresa>> GetById(string id)
    {
        var entity = await _service.GetByIdAsync(id);
        if (entity is null) return NotFound();
        return Ok(entity);
    }

    [HttpPost]
    public async Task<ActionResult<Empresa>> Post(Empresa entity)
    {
        await _service.CreateAsync(entity);
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(string id, Empresa entity)
    {
        var existing = await _service.GetByIdAsync(id);
        if (existing is null) return NotFound();

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
