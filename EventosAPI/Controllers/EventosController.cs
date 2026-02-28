using Microsoft.AspNetCore.Mvc;
using Eventos.Application.Interfaces;
using Eventos.Domain.Entities;

namespace EventosAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class EventosController : ControllerBase
{
    private readonly IEventoService _service;

    public EventosController(IEventoService service)
    {
        _service = service;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Evento>> Get(Guid id)
    {
        var evento = await _service.GetByIdAsync(id);
        if (evento == null) return NotFound();
        return Ok(evento);
    }
}