using Eventos.Application.Enums;
using Eventos.Application.Interfaces;

namespace Eventos.Infrastructure.Reports;

public class RelatorioFactory : IRelatorioFactory
{
    private readonly RelatorioPdfStrategy _pdf;
    private readonly RelatorioExcelStrategy _excel;

    public RelatorioFactory(RelatorioPdfStrategy pdf, RelatorioExcelStrategy excel)
    {
        _pdf = pdf;
        _excel = excel;
    }

    public IRelatorioStrategy Criar(FormatoRelatorio formato) => formato switch
    {
        FormatoRelatorio.Pdf   => _pdf,
        FormatoRelatorio.Excel => _excel,
        _ => throw new ArgumentOutOfRangeException(nameof(formato), $"Formato de relatório não suportado: {formato}")
    };
}
