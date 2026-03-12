using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Eventos.Application.Interfaces;
using Eventos.Application.DTOs.Request;
using Eventos.Application.DTOs.Response;
using EventosAPI.Reports;

namespace EventosAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConvidadoController : ControllerBase
    {
        private readonly IConvidadoService _convidadoService;
        private readonly IAdministracaoService _administracaoService;
        private readonly IRelatorioService _relatorioService;

        public ConvidadoController(
            IConvidadoService convidadoService,
            IAdministracaoService administracaoService,
            IRelatorioService relatorioService)
        {
            _convidadoService = convidadoService;
            _administracaoService = administracaoService;
            _relatorioService = relatorioService;
        }

        /// <summary>
        /// Adiciona um novo convidado ao evento
        /// </summary>
        /// <param name="request">Dados do convidado a ser adicionado</param>
        /// <returns>Resposta com status da operação</returns>
        /// <response code="201">Convidado registrado com sucesso</response>
        /// <response code="400">Dados inválidos ou limite de convidados excedido</response>
        /// <response code="500">Erro interno ao processar a requisição</response>
        [HttpPost("adicionar")]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AdicionarConvidado([FromBody] AdicionarConvidadoRequest request)
        {
            if (request == null)
                return StatusCode(400, new { mensagem = "Dados do convidado são obrigatórios" });

            var response = await _convidadoService.AdicionarConvidadoAsync(request);

            return StatusCode(response.CodigoStatus, response);
        }

        /// <summary>
        /// Verifica se um convidado já está cadastrado pelo nome
        /// </summary>
        /// <param name="nome">Nome (ou parte do nome) do convidado a ser verificado</param>
        /// <returns>Indica se o convidado existe na base</returns>
        /// <response code="200">Consulta realizada com sucesso</response>
        /// <response code="400">Nome não informado</response>
        /// <response code="500">Erro interno ao processar a requisição</response>
        [HttpGet("verificar")]
        [ProducesResponseType(typeof(VerificarConvidadoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(VerificarConvidadoResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(VerificarConvidadoResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> VerificarConvidadoAsync([FromQuery] string nome)
        {
            var response = await _convidadoService.VerificarConvidadoExisteAsync(nome);

            return StatusCode(response.CodigoStatus, response);
        }

        /// <summary>
        /// Lista todos os convidados cadastrados
        /// </summary>
        /// <returns>Lista de convidados com presença, participação e acompanhantes</returns>
        /// <response code="200">Lista retornada com sucesso</response>
        /// <response code="500">Erro interno ao processar a requisição</response>
        [HttpGet("listar")]
        [ProducesResponseType(typeof(List<ConvidadoItem>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ListarConvidados()
        {
            var convidados = await _convidadoService.ListarConvidadosAsync();

            return Ok(convidados);
        }

        /// <summary>
        /// Remove convidados e acompanhantes duplicados (critério: mesmo nome, sem distinção de maiúsculas/minúsculas)
        /// </summary>
        /// <returns>Quantidade de convidados e acompanhantes removidos</returns>
        /// <response code="200">Duplicatas removidas com sucesso</response>
        /// <response code="500">Erro interno ao processar a requisição</response>
        [HttpDelete("remover-duplicatas")]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RemoverDuplicatas()
        {
            var response = await _administracaoService.RemoverDuplicatasAsync();

            return StatusCode(response.CodigoStatus, response);
        }

        /// <summary>
        /// Remove um convidado pelo nome
        /// </summary>
        /// <param name="nome">Nome (ou parte do nome) do convidado a ser removido</param>
        /// <returns>Resposta com status da operação</returns>
        /// <response code="200">Convidado removido com sucesso</response>
        /// <response code="400">Nome não informado ou múltiplos convidados encontrados</response>
        /// <response code="404">Convidado não encontrado</response>
        /// <response code="500">Erro interno ao processar a requisição</response>
        [HttpDelete("remover")]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RemoverConvidado([FromQuery] string nome)
        {
            var response = await _convidadoService.RemoverConvidadoPorNomeAsync(nome);

            return StatusCode(response.CodigoStatus, response);
        }

        /// <summary>
        /// Zera todas as tabelas do banco de dados
        /// </summary>
        /// <returns>Resposta com status da operação</returns>
        /// <response code="200">Tabelas zeradas com sucesso</response>
        /// <response code="500">Erro interno ao processar a requisição</response>
        [HttpDelete("zerar-tabelas")]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ZerarTabelas()
        {
            var response = await _administracaoService.ZerarTabelasAsync();

            return StatusCode(response.CodigoStatus, response);
        }

        /// <summary>
        /// Exporta o relatório de convidados confirmados em formato Excel (.xlsx)
        /// </summary>
        /// <returns>Arquivo Excel com a relação de participantes e seus acompanhantes</returns>
        /// <response code="200">Arquivo gerado com sucesso</response>
        /// <response code="500">Erro interno ao processar a requisição</response>
        [HttpGet("relatorio/excel")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ObterRelatorioExcel()
        {
            var (bytes, contentType, nomeArquivo) = await _relatorioService.ExportarAsync(new RelatorioExcelExporter());
            return File(bytes, contentType, nomeArquivo);
        }

        /// <summary>
        /// Exporta o relatório de convidados confirmados em formato PDF
        /// </summary>
        /// <returns>Arquivo PDF com a relação de participantes e seus acompanhantes</returns>
        /// <response code="200">Arquivo gerado com sucesso</response>
        /// <response code="500">Erro interno ao processar a requisição</response>
        [HttpGet("relatorio/pdf")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ObterRelatorioPdf()
        {
            var (bytes, contentType, nomeArquivo) = await _relatorioService.ExportarAsync(new RelatorioPdfExporter());
            return File(bytes, contentType, nomeArquivo);
        }
    }
}
