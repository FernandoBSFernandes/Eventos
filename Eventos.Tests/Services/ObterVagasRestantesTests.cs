namespace Eventos.Tests.Services;

[Trait("Classe", "ConvidadoService")]
[Trait("Serviço", "ObterVagasRestantes")]
public class ObterVagasRestantesTests : ConvidadoServiceTestBase
{
    #region Sucesso

    [Fact(DisplayName = "Deve retornar 105 vagas quando nenhuma pessoa está confirmada")]
    [Trait("Categoria", "Sucesso")]
    public async Task DeveRetornar105Vagas_QuandoNenhumaPessoaConfirmada()
    {
        // Arrange
        Repo.ObterTotalPessoasAsync().Returns(0);

        // Act
        var response = await Service.ObterVagasRestantesAsync();

        // Assert
        Assert.Equal(200, response.CodigoStatus);
        Assert.Equal(105, response.VagasRestantes);
    }

    [Fact(DisplayName = "Deve retornar vagas corretas quando há pessoas confirmadas")]
    [Trait("Categoria", "Sucesso")]
    public async Task DeveRetornarVagasCorretas_QuandoHaPessoasConfirmadas()
    {
        // Arrange
        Repo.ObterTotalPessoasAsync().Returns(63);

        // Act
        var response = await Service.ObterVagasRestantesAsync();

        // Assert
        Assert.Equal(200, response.CodigoStatus);
        Assert.Equal(42, response.VagasRestantes);
    }

    [Fact(DisplayName = "Deve retornar zero vagas quando limite é atingido")]
    [Trait("Categoria", "Limite")]
    public async Task DeveRetornarZeroVagas_QuandoLimiteAtingido()
    {
        // Arrange
        Repo.ObterTotalPessoasAsync().Returns(105);

        // Act
        var response = await Service.ObterVagasRestantesAsync();

        // Assert
        Assert.Equal(200, response.CodigoStatus);
        Assert.Equal(0, response.VagasRestantes);
    }

    [Fact(DisplayName = "Deve retornar zero vagas quando limite é ultrapassado")]
    [Trait("Categoria", "Limite")]
    public async Task DeveRetornarZeroVagas_QuandoUltrapassaLimite()
    {
        // Arrange Ã¢Â€Â” cenário de dados legados com mais de 105 pessoas
        Repo.ObterTotalPessoasAsync().Returns(110);

        // Act
        var response = await Service.ObterVagasRestantesAsync();

        // Assert
        Assert.Equal(200, response.CodigoStatus);
        Assert.Equal(0, response.VagasRestantes);
    }

    [Fact(DisplayName = "Deve retornar 1 vaga quando falta 1 pessoa para o limite")]
    [Trait("Categoria", "Limite")]
    public async Task DeveRetornar1Vaga_QuandoFalta1PessoaParaOLimite()
    {
        // Arrange
        Repo.ObterTotalPessoasAsync().Returns(104);

        // Act
        var response = await Service.ObterVagasRestantesAsync();

        // Assert
        Assert.Equal(200, response.CodigoStatus);
        Assert.Equal(1, response.VagasRestantes);
    }

    #endregion

    #region Erro Interno

    [Fact(DisplayName = "Deve retornar 500 quando repositório lança exceção")]
    [Trait("Categoria", "Erro Interno")]
    public async Task DeveRetornar500_QuandoRepositorioLancaExcecao()
    {
        // Arrange
        Repo.ObterTotalPessoasAsync()
            .Throws(new Exception("Erro na base de dados"));

        // Act
        var response = await Service.ObterVagasRestantesAsync();

        // Assert
        Assert.Equal(500, response.CodigoStatus);
        Assert.Equal(0, response.VagasRestantes);
    }

    #endregion
}
