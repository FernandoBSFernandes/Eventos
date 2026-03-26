using Eventos.IntegrationTests.Base;

namespace Eventos.IntegrationTests.Controllers;

[Collection(IntegrationTestCollection.Name)]
[Trait("Integração", "ConvidadoController")]
[Trait("Classe", "ConvidadoController")]
public class ConvidadoControllerTests : IntegrationTestBase
{
    public ConvidadoControllerTests(EventosWebApplicationFactory factory) : base(factory) { }

    #region POST /api/convidado/adicionar

    [Fact(DisplayName = "Deve retornar 201 ao adicionar convidado sozinho com dados válidos")]
    [Trait("Categoria", "Sucesso")]
    public async Task AdicionarConvidado_DeveRetornar201_QuandoDadosValidos()
    {
        // Arrange
        var request = new AdicionarConvidadoRequest(
            nome: "João Silva",
            presencaConfirmada: true,
            participacao: Participacao.Sozinho,
            quantidadeAcompanhantes: 0,
            nomesAcompanhantes: new List<string>());

        // Act
        var response = await Client.PostAsJsonAsync("/api/convidado/adicionar", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<BaseResponse>();
        Assert.NotNull(body);
        Assert.Equal(201, body.CodigoStatus);
        Assert.NotEmpty(body.Mensagem);
    }

    [Fact(DisplayName = "Deve retornar 201 ao adicionar convidado acompanhado com dados válidos")]
    [Trait("Categoria", "Sucesso")]
    public async Task AdicionarConvidado_DeveRetornar201_QuandoAcompanhadoComDadosValidos()
    {
        // Arrange
        var request = new AdicionarConvidadoRequest(
            nome: "Maria Santos",
            presencaConfirmada: true,
            participacao: Participacao.Acompanhado,
            quantidadeAcompanhantes: 2,
            nomesAcompanhantes: new List<string> { "Ana Costa", "Pedro Costa" });

        // Act
        var response = await Client.PostAsJsonAsync("/api/convidado/adicionar", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<BaseResponse>();
        Assert.Equal(201, body!.CodigoStatus);
    }

    [Theory(DisplayName = "Deve retornar 400 quando nome é inválido")]
    [Trait("Categoria", "Validação")]
    [InlineData("")]
    [InlineData("   ")]
    public async Task AdicionarConvidado_DeveRetornar400_QuandoNomeInvalido(string nome)
    {
        // Arrange
        var request = new AdicionarConvidadoRequest(
            nome: nome,
            presencaConfirmada: true,
            participacao: Participacao.Sozinho,
            quantidadeAcompanhantes: 0,
            nomesAcompanhantes: new List<string>());

        // Act
        var response = await Client.PostAsJsonAsync("/api/convidado/adicionar", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact(DisplayName = "Deve retornar 400 quando quantidade de acompanhantes não corresponde aos nomes")]
    [Trait("Categoria", "Validação")]
    public async Task AdicionarConvidado_DeveRetornar400_QuandoQuantidadeNaoCorrespondeAOsNomes()
    {
        // Arrange
        var request = new AdicionarConvidadoRequest(
            nome: "Carlos Lima",
            presencaConfirmada: true,
            participacao: Participacao.Acompanhado,
            quantidadeAcompanhantes: 3,
            nomesAcompanhantes: new List<string> { "Acomp Um" });

        // Act
        var response = await Client.PostAsJsonAsync("/api/convidado/adicionar", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region GET /api/convidado/listar

    [Fact(DisplayName = "Deve retornar lista vazia quando nenhum convidado foi adicionado")]
    [Trait("Categoria", "Sucesso")]
    public async Task ListarConvidados_DeveRetornarListaVazia_QuandoNenhumConvidado()
    {
        // Act
        var response = await Client.GetAsync("/api/convidado/listar");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<List<ConvidadoItem>>();
        Assert.NotNull(body);
        Assert.Empty(body);
    }

    [Fact(DisplayName = "Deve retornar convidado recém-adicionado na listagem")]
    [Trait("Categoria", "Sucesso")]
    public async Task ListarConvidados_DeveRetornarConvidadoRecem_QuandoAdicionadoAntes()
    {
        // Arrange
        var request = new AdicionarConvidadoRequest(
            nome: "Fernanda Rocha",
            presencaConfirmada: true,
            participacao: Participacao.Sozinho,
            quantidadeAcompanhantes: 0,
            nomesAcompanhantes: new List<string>());

        await Client.PostAsJsonAsync("/api/convidado/adicionar", request);

        // Act
        var response = await Client.GetAsync("/api/convidado/listar");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<List<ConvidadoItem>>();
        Assert.NotNull(body);
        Assert.Single(body);
        Assert.Equal("Fernanda Rocha", body[0].Nome);
    }

    #endregion

    #region GET /api/convidado/verificar

    [Fact(DisplayName = "Deve retornar existeComoConvidado=true quando convidado está cadastrado")]
    [Trait("Categoria", "Sucesso")]
    public async Task VerificarConvidado_DeveRetornarExisteTrue_QuandoCadastrado()
    {
        // Arrange
        var request = new AdicionarConvidadoRequest(
            nome: "Lucas Almeida",
            presencaConfirmada: true,
            participacao: Participacao.Sozinho,
            quantidadeAcompanhantes: 0,
            nomesAcompanhantes: new List<string>());

        await Client.PostAsJsonAsync("/api/convidado/adicionar", request);

        // Act
        var response = await Client.GetAsync("/api/convidado/verificar?nome=Lucas");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<VerificarConvidadoResponse>();
        Assert.NotNull(body);
        Assert.True(body.ExisteComoConvidado);
        Assert.False(body.ExisteComoAcompanhante);
    }

    [Fact(DisplayName = "Deve retornar ambas as flags false quando convidado não está cadastrado")]
    [Trait("Categoria", "Sucesso")]
    public async Task VerificarConvidado_DeveRetornarExisteFalse_QuandoNaoCadastrado()
    {
        // Act
        var response = await Client.GetAsync("/api/convidado/verificar?nome=Inexistente");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<VerificarConvidadoResponse>();
        Assert.NotNull(body);
        Assert.False(body.ExisteComoConvidado);
        Assert.False(body.ExisteComoAcompanhante);
    }

    [Fact(DisplayName = "Deve retornar existeComoAcompanhante=true quando nome é encontrado como acompanhante")]
    [Trait("Categoria", "Sucesso")]
    public async Task VerificarConvidado_DeveRetornarExisteTrue_QuandoEncontradoComoAcompanhante()
    {
        // Arrange Ã¢Â€Â” adiciona convidado com acompanhante, mas busca pelo nome do acompanhante
        var request = new AdicionarConvidadoRequest(
            nome: "Titular Souza",
            presencaConfirmada: true,
            participacao: Participacao.Acompanhado,
            quantidadeAcompanhantes: 1,
            nomesAcompanhantes: new List<string> { "Acompanhante Souza" });

        await Client.PostAsJsonAsync("/api/convidado/adicionar", request);

        // Act
        var response = await Client.GetAsync("/api/convidado/verificar?nome=Acompanhante");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<VerificarConvidadoResponse>();
        Assert.NotNull(body);
        Assert.False(body.ExisteComoConvidado);
        Assert.True(body.ExisteComoAcompanhante);
    }

    [Fact(DisplayName = "Deve retornar 400 quando nome não é informado")]
    [Trait("Categoria", "Validação")]
    public async Task VerificarConvidado_DeveRetornar400_QuandoNomeNaoInformado()
    {
        // Act
        var response = await Client.GetAsync("/api/convidado/verificar");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region DELETE /api/convidado/remover

    [Fact(DisplayName = "Deve retornar 200 quando convidado é removido com sucesso")]
    [Trait("Categoria", "Sucesso")]
    public async Task RemoverConvidado_DeveRetornar200_QuandoConvidadoExiste()
    {
        // Arrange
        var request = new AdicionarConvidadoRequest(
            nome: "Beatriz Nunes",
            presencaConfirmada: true,
            participacao: Participacao.Sozinho,
            quantidadeAcompanhantes: 0,
            nomesAcompanhantes: new List<string>());

        await Client.PostAsJsonAsync("/api/convidado/adicionar", request);

        // Act
        var response = await Client.DeleteAsync("/api/convidado/remover?nome=Beatriz Nunes");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<BaseResponse>();
        Assert.NotNull(body);
        Assert.NotEmpty(body.Mensagem);
    }

    [Fact(DisplayName = "Deve retornar 404 quando convidado não existe")]
    [Trait("Categoria", "Não Encontrado")]
    public async Task RemoverConvidado_DeveRetornar404_QuandoConvidadoNaoExiste()
    {
        // Act
        var response = await Client.DeleteAsync("/api/convidado/remover?nome=Ninguem");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact(DisplayName = "Deve retornar 400 quando nome não é informado")]
    [Trait("Categoria", "Validação")]
    public async Task RemoverConvidado_DeveRetornar400_QuandoNomeNaoInformado()
    {
        // Act
        var response = await Client.DeleteAsync("/api/convidado/remover");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact(DisplayName = "Deve retornar 400 quando múltiplos convidados com nome semelhante são encontrados")]
    [Trait("Categoria", "Validação")]
    public async Task RemoverConvidado_DeveRetornar400_QuandoMultiplosConvidadosEncontrados()
    {
        // Arrange Ã¢Â€Â” adiciona dois convidados com nomes semelhantes
        await Client.PostAsJsonAsync("/api/convidado/adicionar",
            new AdicionarConvidadoRequest("Ana Santos", true, Participacao.Sozinho, 0, new List<string>()));
        await Client.PostAsJsonAsync("/api/convidado/adicionar",
            new AdicionarConvidadoRequest("Ana Costa", true, Participacao.Sozinho, 0, new List<string>()));

        // Act Ã¢Â€Â” busca parcial que retorna ambos
        var response = await Client.DeleteAsync("/api/convidado/remover?nome=Ana");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<BaseResponse>();
        Assert.NotNull(body);
        Assert.NotEmpty(body!.Mensagem);
    }

    #endregion

    #region GET /api/convidado/vagas-restantes

    [Fact(DisplayName = "Deve retornar vagas restantes corretamente após adição de convidados")]
    [Trait("Categoria", "Sucesso")]
    public async Task VagasRestantes_DeveRetornarVagasCorretas_QuandoHaConvidadosConfirmados()
    {
        // Arrange Ã¢Â€Â” adiciona um convidado confirmado com 2 acompanhantes (3 pessoas)
        var request = new AdicionarConvidadoRequest(
            nome: "Rafael Gomes",
            presencaConfirmada: true,
            participacao: Participacao.Acompanhado,
            quantidadeAcompanhantes: 2,
            nomesAcompanhantes: new List<string> { "Acomp Um", "Acomp Doi" });

        await Client.PostAsJsonAsync("/api/convidado/adicionar", request);

        // Act
        var response = await Client.GetAsync("/api/convidado/vagas-restantes");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<VagasRestantesResponse>();
        Assert.NotNull(body);
        Assert.Equal(102, body.VagasRestantes);
    }

    [Fact(DisplayName = "Deve retornar 105 vagas quando banco está vazio")]
    [Trait("Categoria", "Sucesso")]
    public async Task VagasRestantes_DeveRetornar105Vagas_QuandoBancoVazio()
    {
        // Act
        var response = await Client.GetAsync("/api/convidado/vagas-restantes");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<VagasRestantesResponse>();
        Assert.NotNull(body);
        Assert.Equal(105, body.VagasRestantes);
    }

    #endregion
}
