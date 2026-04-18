namespace Eventos.Tests.Services;

[Trait("Classe", "ConvidadoService")]
[Trait("Serviço", "ObterLimiteMaximoPessoas")]
public class ObterLimiteMaximoPessoasTests : ConvidadoServiceTestBase
{
    [Fact(DisplayName = "Deve retornar o limite máximo configurado")]
    [Trait("Categoria", "Sucesso")]
    public async Task DeveRetornarLimiteMaximoConfigurado()
    {
        // Act
        var response = await Service.ObterLimiteMaximoPessoasAsync();

        // Assert
        Assert.Equal(200, response.CodigoStatus);
        Assert.Equal(105, response.LimiteMaximoPessoas);
    }
}
