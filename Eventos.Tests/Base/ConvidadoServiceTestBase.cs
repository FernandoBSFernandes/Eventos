using Eventos.Application.Configuration;
using Eventos.Application.Services;
using Eventos.Domain.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Eventos.Tests.Base;

public abstract class ConvidadoServiceTestBase
{
    protected readonly IEventoRepository Repo;
    protected readonly ConvidadoService Service;

    protected ConvidadoServiceTestBase()
    {
        Repo = Substitute.For<IEventoRepository>();
        var options = Options.Create(new EventoConfiguration { LimiteMaximoPessoas = 100 });
        Service = new ConvidadoService(Repo, NullLogger<ConvidadoService>.Instance, options);
    }
}
