using ClosedXML.Excel;
using Eventos.Application.DTOs.Response;

namespace EventosAPI.Reports;

public static class RelatorioExcelGenerator
{
    public static byte[] Gerar(RelatorioEventoResponse relatorio)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Relatório");

        // Cabeçalho principal
        var titulo = sheet.Range("A1:C1").Merge();
        titulo.Value = "Relatório de Convidados Confirmados";
        titulo.Style.Font.Bold = true;
        titulo.Style.Font.FontSize = 14;
        titulo.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        titulo.Style.Fill.BackgroundColor = XLColor.FromHtml("#2E75B6");
        titulo.Style.Font.FontColor = XLColor.White;

        // Cabeçalhos das colunas
        sheet.Cell("A2").Value = "Convidado";
        sheet.Cell("B2").Value = "Acompanhantes";
        sheet.Cell("C2").Value = "Qtd. Acompanhantes";

        var cabecalho = sheet.Range("A2:C2");
        cabecalho.Style.Font.Bold = true;
        cabecalho.Style.Fill.BackgroundColor = XLColor.FromHtml("#D6E4F0");
        cabecalho.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        // Dados
        int linha = 3;
        foreach (var convidado in relatorio.Convidados)
        {
            sheet.Cell(linha, 1).Value = convidado.Nome;
            sheet.Cell(linha, 2).Value = convidado.Acompanhantes.Count > 0
                ? string.Join(", ", convidado.Acompanhantes)
                : "—";
            sheet.Cell(linha, 3).Value = convidado.Acompanhantes.Count;
            sheet.Cell(linha, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            if (linha % 2 == 0)
                sheet.Range(linha, 1, linha, 3).Style.Fill.BackgroundColor = XLColor.FromHtml("#F2F7FC");

            linha++;
        }

        // Linha de total
        sheet.Cell(linha, 1).Value = "Total de pessoas no evento:";
        sheet.Cell(linha, 1).Style.Font.Bold = true;
        sheet.Cell(linha, 3).Value = relatorio.TotalPessoas;
        sheet.Cell(linha, 3).Style.Font.Bold = true;
        sheet.Cell(linha, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        sheet.Range(linha, 1, linha, 3).Style.Fill.BackgroundColor = XLColor.FromHtml("#D6E4F0");

        sheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
