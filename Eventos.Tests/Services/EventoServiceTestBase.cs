using Eventos.Application.Services;
using Eventos.Domain.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Eventos.Tests.Services;

public abstract class ConvidadoServiceTestBase
{
    protected readonly IEventoRepository Repo;
    protected readonly ConvidadoService Service;

    protected ConvidadoServiceTestBase()
    {
        Repo = Substitute.For<IEventoRepository>();
        Service = new ConvidadoService(Repo, NullLogger<ConvidadoService>.Instance);
    }
}

public abstract class AdministracaoServiceTestBase
{
    protected readonly IEventoRepository Repo;
    protected readonly AdministracaoService Service;

    protected AdministracaoServiceTestBase()
    {
        Repo = Substitute.For<IEventoRepository>();
        Service = new AdministracaoService(Repo, NullLogger<AdministracaoService>.Instance);
    }
}

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
