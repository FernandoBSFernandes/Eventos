using Xunit;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Eventos.Tests.Services;

public class ObterVagasRestantesTests : ConvidadoServiceTestBase
{
    #region Sucesso

    [Fact]
    public async Task DeveRetornar100Vagas_QuandoNenhumaPessoaConfirmada()
    {
        // Arrange
        Repo.ObterTotalPessoasAsync().Returns(0);

        // Act
        var response = await Service.ObterVagasRestantesAsync();

        // Assert
        Assert.Equal(200, response.CodigoStatus);
        Assert.Equal(100, response.VagasRestantes);
        Assert.Equal(0, response.PessoasConfirmadas);
    }

    [Fact]
    public async Task DeveRetornarVagasCorretas_QuandoHaPessoasConfirmadas()
    {
        // Arrange
        Repo.ObterTotalPessoasAsync().Returns(63);

        // Act
        var response = await Service.ObterVagasRestantesAsync();

        // Assert
        Assert.Equal(200, response.CodigoStatus);
        Assert.Equal(37, response.VagasRestantes);
        Assert.Equal(63, response.PessoasConfirmadas);
    }

    [Fact]
    public async Task DeveRetornarZeroVagas_QuandoLimiteAtingido()
    {
        // Arrange
        Repo.ObterTotalPessoasAsync().Returns(100);

        // Act
        var response = await Service.ObterVagasRestantesAsync();

        // Assert
        Assert.Equal(200, response.CodigoStatus);
        Assert.Equal(0, response.VagasRestantes);
        Assert.Equal(100, response.PessoasConfirmadas);
    }

    [Fact]
    public async Task DeveRetornarZeroVagas_QuandoUltrapassaLimite()
    {
        // Arrange — cenário de dados legados com mais de 100 pessoas
        Repo.ObterTotalPessoasAsync().Returns(105);

        // Act
        var response = await Service.ObterVagasRestantesAsync();

        // Assert
        Assert.Equal(200, response.CodigoStatus);
        Assert.Equal(0, response.VagasRestantes);
        Assert.Equal(105, response.PessoasConfirmadas);
    }

    [Fact]
    public async Task DeveRetornar1Vaga_QuandoFalta1PessoaParaOLimite()
    {
        // Arrange
        Repo.ObterTotalPessoasAsync().Returns(99);

        // Act
        var response = await Service.ObterVagasRestantesAsync();

        // Assert
        Assert.Equal(200, response.CodigoStatus);
        Assert.Equal(1, response.VagasRestantes);
        Assert.Equal(99, response.PessoasConfirmadas);
    }

    #endregion

    #region Erro Interno

    [Fact]
    public async Task DeveRetornar500_QuandoRepositorioLancaExcecao()
    {
        // Arrange
        Repo.ObterTotalPessoasAsync()
            .Throws(new Exception("Erro na base de dados"));

        // Act
        var response = await Service.ObterVagasRestantesAsync();

        // Assert
        Assert.Equal(500, response.CodigoStatus);
        Assert.Equal("Ocorreu um erro interno. Tente novamente mais tarde.", response.Mensagem);
        Assert.Equal(0, response.VagasRestantes);
        Assert.Equal(0, response.PessoasConfirmadas);
    }

    #endregion
}
