using Eventos.Application.Services;
using Eventos.Domain.Repositories;
using Microsoft.Extensions.Logging.Abstractions;

namespace Eventos.Tests.Services;

public abstract class RelatorioServiceTestBase
{
    protected readonly IEventoRepository Repo;
    protected readonly RelatorioService Service;

    protected RelatorioServiceTestBase()
    {
        Repo = Substitute.For<IEventoRepository>();
        Service = new RelatorioService(Repo, NullLogger<RelatorioService>.Instance);
    }
}
