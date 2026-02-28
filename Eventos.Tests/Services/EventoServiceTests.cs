using Xunit;
using NSubstitute;
using Eventos.Application.Services;
using Eventos.Domain.Repositories;
using Eventos.Domain.Entities;

namespace Eventos.Tests.Services;

public class EventoServiceTests
{
    [Fact]
    public async Task GetById_Returns_Evento_When_Found()
    {
        var id = Guid.NewGuid();
        var evento = new Evento(id, "Teste", DateTime.UtcNow);

        var repo = Substitute.For<IEventoRepository>();
        repo.GetByIdAsync(id).Returns(Task.FromResult<Evento?>(evento));

        var service = new EventoService(repo);

        var result = await service.GetByIdAsync(id);

        Assert.NotNull(result);
        Assert.Equal(id, result!.Id);
    }

    [Fact]
    public async Task GetById_Returns_Null_When_NotFound()
    {
        var id = Guid.NewGuid();
        var repo = Substitute.For<IEventoRepository>();
        repo.GetByIdAsync(id).Returns(Task.FromResult<Evento?>(null));

        var service = new EventoService(repo);

        var result = await service.GetByIdAsync(id);

        Assert.Null(result);
    }
}