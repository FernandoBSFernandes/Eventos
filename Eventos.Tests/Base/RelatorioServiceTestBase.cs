using Eventos.Application.Interfaces;
using Eventos.Application.Services;
using Eventos.Domain.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using Eventos.Application.DTOs.Response;

namespace Eventos.Tests.Base;

public abstract class RelatorioServiceTestBase
{
    protected readonly IEventoRepository Repo;
    protected readonly IRelatorioFactory Factory;
    protected readonly IListaFinalConfirmadosPdfStrategy ListaFinalConfirmadosPdfStrategy;
    protected readonly RelatorioService Service;

    protected RelatorioServiceTestBase()
    {
        Repo = Substitute.For<IEventoRepository>();
        Factory = Substitute.For<IRelatorioFactory>();
        ListaFinalConfirmadosPdfStrategy = Substitute.For<IListaFinalConfirmadosPdfStrategy>();
        ListaFinalConfirmadosPdfStrategy.ContentType.Returns("application/pdf");
        ListaFinalConfirmadosPdfStrategy.NomeArquivo.Returns("Lista Final de Confirmados.pdf");
        ListaFinalConfirmadosPdfStrategy.ExportarAsync(Arg.Any<ListaFinalConfirmadosResponse>())
            .Returns(Array.Empty<byte>());

        Service = new RelatorioService(
            Repo,
            NullLogger<RelatorioService>.Instance,
            Factory,
            ListaFinalConfirmadosPdfStrategy);
    }
}
