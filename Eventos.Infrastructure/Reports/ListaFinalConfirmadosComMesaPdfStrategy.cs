using Eventos.Application.DTOs.Response;
using Eventos.Application.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Eventos.Infrastructure.Reports;

public class ListaFinalConfirmadosComMesaPdfStrategy : IListaFinalConfirmadosComMesaPdfStrategy
{
    public string ContentType => "application/pdf";
    public string NomeArquivo => "Lista Final de Confirmados com Mesa.pdf";

    public Task<byte[]> ExportarAsync(ListaFinalConfirmadosResponse listaFinal)
    {
        var bytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.MarginHorizontal(1.2f, Unit.Centimetre);
                page.MarginTop(1.5f, Unit.Centimetre);
                page.MarginBottom(1f, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Segoe UI"));

                page.Header().Element(header =>
                {
                    header.Column(col =>
                    {
                        col.Item().Text("Lista Final de Confirmados - Casamento Fernando e Suzana - 25/04/2025")
                            .FontSize(18).Bold().FontColor(Colors.Blue.Darken2).AlignCenter();

                        col.Item().PaddingTop(4).LineHorizontal(1).LineColor(Colors.Blue.Lighten2);
                    });
                });

                page.Content().Element(content =>
                {
                    content.PaddingTop(10).Column(col =>
                    {
                        if (listaFinal.Confirmados.Count == 0)
                        {
                            col.Item().Text("Nenhum confirmado encontrado.").FontColor(Colors.Grey.Medium);
                            return;
                        }

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(50);
                                columns.RelativeColumn(6);
                                columns.ConstantColumn(80);
                                columns.ConstantColumn(60);
                            });

                            table.Header(header =>
                            {
                                foreach (var titulo in new[] { "Nº", "Nome", "Mesa", "Pago" })
                                {
                                    header.Cell().Background(Colors.Blue.Darken2).Padding(6)
                                        .Text(titulo).Bold().FontColor(Colors.White).AlignCenter();
                                }
                            });

                            for (int i = 0; i < listaFinal.Confirmados.Count; i++)
                            {
                                var confirmado = listaFinal.Confirmados[i];
                                var cor = i % 2 == 0 ? Colors.White : Colors.Blue.Lighten5;

                                table.Cell().Background(cor).Padding(5).Text(confirmado.Numero.ToString()).AlignCenter();
                                table.Cell().Background(cor).Padding(5).Text(confirmado.Nome);
                                table.Cell().Background(cor).Padding(5).Text(confirmado.Mesa ?? string.Empty).AlignCenter();
                                table.Cell().Background(cor).Padding(5).AlignCenter().Element(celula =>
                                    celula
                                        .Width(14)
                                        .Height(14)
                                        .Border(1.2f)
                                        .BorderColor(Colors.Black)
                                        .AlignCenter()
                                        .AlignMiddle()
                                        .Text(confirmado.Pago == true ? "X" : string.Empty)
                                        .FontSize(9)
                                        .Bold());
                            }
                        });
                    });
                });
            });
        }).GeneratePdf();

        return Task.FromResult(bytes);
    }
}
