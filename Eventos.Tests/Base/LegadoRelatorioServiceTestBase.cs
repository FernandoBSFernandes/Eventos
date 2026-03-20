using Eventos.Application.Services;
using Eventos.Domain.Repositories;
using Microsoft.Extensions.Logging.Abstractions;

namespace Eventos.Tests.Base;

public abstract class LegadoRelatorioServiceTestBase
{
    protected readonly IOrigemRepository Repo;
    protected readonly LegadoRelatorioService Service;

    protected LegadoRelatorioServiceTestBase()
    {
        Repo = Substitute.For<IOrigemRepository>();
        Service = new LegadoRelatorioService(Repo, NullLogger<LegadoRelatorioService>.Instance);
    }
}
