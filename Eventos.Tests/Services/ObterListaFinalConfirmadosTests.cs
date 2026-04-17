using Eventos.Application.DTOs.Response;

namespace Eventos.Tests.Services;

[Trait("Classe", "RelatorioService")]
[Trait("Serviço", "ObterListaFinalConfirmados")]
public class ObterListaFinalConfirmadosTests : RelatorioServiceTestBase
{
    [Fact(DisplayName = "Deve retornar nomes confirmados em ordem alfabética com campo pago vazio")]
    [Trait("Categoria", "Sucesso")]
    public async Task DeveRetornarListaOrdenadaComPagoNulo_QuandoHaConfirmados()
    {
        // Arrange
        var convidados = new List<Convidado>
        {
            new()
            {
                Nome = "Carlos",
                PresencaConfirmada = true,
                Acompanhantes = new List<Acompanhante>
                {
                    new() { Nome = "Bruna" }
                }
            },
            new()
            {
                Nome = "Ana",
                PresencaConfirmada = true,
                Acompanhantes = new List<Acompanhante>()
            }
        };

        Repo.ObterConvidadosConfirmadosAsync().Returns(convidados);

        // Act
        var response = await Service.ObterListaFinalConfirmadosAsync();

        // Assert
        Assert.Equal(200, response.CodigoStatus);
        Assert.Equal(3, response.Confirmados.Count);
        Assert.Equal(new[] { 1, 2, 3 }, response.Confirmados.Select(x => x.Numero).ToArray());
        Assert.Equal(new[] { "Ana", "Bruna", "Carlos" }, response.Confirmados.Select(x => x.Nome).ToArray());
        Assert.All(response.Confirmados, x => Assert.Null(x.Pago));
    }

    [Fact(DisplayName = "Deve retornar lista vazia quando não houver confirmados")]
    [Trait("Categoria", "Sucesso")]
    public async Task DeveRetornarListaVazia_QuandoNaoHaConfirmados()
    {
        // Arrange
        Repo.ObterConvidadosConfirmadosAsync().Returns(new List<Convidado>());

        // Act
        var response = await Service.ObterListaFinalConfirmadosAsync();

        // Assert
        Assert.Equal(200, response.CodigoStatus);
        Assert.Empty(response.Confirmados);
        Assert.Equal("Nenhum confirmado encontrado.", response.Mensagem);
    }
}
