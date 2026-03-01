using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Eventos.Application.DTOs.Response;

namespace EventosAPI.Reports;

public static class RelatorioPdfGenerator
{
    public static Task<byte[]> GerarAsync(RelatorioEventoResponse relatorio)
    {
        return Task.Run(() => Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                page.Header().Element(ComposeHeader);
                page.Content().Element(content => ComposeContent(content, relatorio));
                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Gerado em: ").FontSize(9).FontColor(Colors.Grey.Medium);
                    text.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm")).FontSize(9).FontColor(Colors.Grey.Medium);
                });
            });
        }).GeneratePdf());
    }

    private static void ComposeHeader(IContainer container)
    {
        container.PaddingBottom(10).Column(col =>
        {
            col.Item().Text("Relatório de Convidados Confirmados")
                .FontSize(18).Bold().FontColor(Colors.Blue.Darken2).AlignCenter();

            col.Item().PaddingTop(4).LineHorizontal(1).LineColor(Colors.Blue.Lighten2);
        });
    }

    private static void ComposeContent(IContainer container, RelatorioEventoResponse relatorio)
    {
        container.PaddingTop(10).Column(col =>
        {
            // Tabela de convidados
            col.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);
                    columns.RelativeColumn(5);
                    columns.RelativeColumn(2);
                });

                // Cabeçalho da tabela
                table.Header(header =>
                {
                    foreach (var titulo in new[] { "Convidado", "Acompanhantes", "Qtd." })
                    {
                        header.Cell().Background(Colors.Blue.Darken2).Padding(6).Text(titulo)
                            .Bold().FontColor(Colors.White).AlignCenter();
                    }
                });

                // Linhas de dados
                for (int i = 0; i < relatorio.Convidados.Count; i++)
                {
                    var convidado = relatorio.Convidados[i];
                    var cor = i % 2 == 0 ? Colors.White : Colors.Blue.Lighten5;

                    table.Cell().Background(cor).Padding(5).Text(convidado.Nome);
                    table.Cell().Background(cor).Padding(5).Text(
                        convidado.Acompanhantes.Count > 0
                            ? string.Join(", ", convidado.Acompanhantes)
                            : "—"
                    ).FontColor(convidado.Acompanhantes.Count > 0 ? Colors.Black : Colors.Grey.Medium);
                    table.Cell().Background(cor).Padding(5).Text(convidado.Acompanhantes.Count.ToString()).AlignCenter();
                }
            });

            // Linha de total
            col.Item().PaddingTop(16).Row(row =>
            {
                row.RelativeItem().Background(Colors.Blue.Lighten4).Padding(8)
                    .Text("Total de pessoas no evento:").Bold();
                row.ConstantItem(80).Background(Colors.Blue.Lighten4).Padding(8)
                    .Text(relatorio.TotalPessoas.ToString()).Bold().AlignCenter();
            });
        });
    }
}
