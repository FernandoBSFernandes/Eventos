using Eventos.Application.Interfaces;
using Eventos.Application.Services;
using Eventos.Domain.Repositories;
using Microsoft.Extensions.Logging.Abstractions;

namespace Eventos.Tests.Base;

public abstract class RelatorioServiceTestBase
{
    protected readonly IEventoRepository Repo;
    protected readonly IRelatorioFactory Factory;
    protected readonly RelatorioService Service;

    protected RelatorioServiceTestBase()
    {
        Repo = Substitute.For<IEventoRepository>();
        Factory = Substitute.For<IRelatorioFactory>();
        Service = new RelatorioService(Repo, NullLogger<RelatorioService>.Instance, Factory);
    }
}
