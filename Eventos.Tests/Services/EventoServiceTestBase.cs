using Eventos.Application.Services;
using Eventos.Domain.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Eventos.Tests.Services;

public abstract class EventoServiceTestBase
{
    protected readonly IEventoRepository Repo;
    protected readonly EventoService Service;

    protected EventoServiceTestBase()
    {
        Repo = Substitute.For<IEventoRepository>();
        Service = new EventoService(Repo, NullLogger<EventoService>.Instance);
    }
}
