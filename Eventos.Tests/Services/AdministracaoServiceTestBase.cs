using Eventos.Application.Services;
using Eventos.Domain.Repositories;
using Microsoft.Extensions.Logging.Abstractions;

namespace Eventos.Tests.Services;

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
