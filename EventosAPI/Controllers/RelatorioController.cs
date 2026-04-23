using Eventos.Application.Interfaces;
using Eventos.Application.DTOs.Response;
using Microsoft.AspNetCore.Mvc;

namespace EventosAPI.Controllers
{
    /// <summary>
    /// Geração de relatórios do evento.
    /// Permite exportar a lista de convidados confirmados em PDF.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Consumes("application/json")]
    [Tags("Relatórios")]
    public class RelatorioController : ControllerBase
    {
        private readonly IRelatorioService _relatorioService;

        public RelatorioController(IRelatorioService relatorioService)
        {
            _relatorioService = relatorioService;
        }

        /// <summary>
        /// Exporta o relatório de convidados confirmados em formato PDF
        /// </summary>
        /// <returns>Arquivo PDF com a relação de participantes e seus acompanhantes</returns>
        /// <response code="200">Arquivo gerado com sucesso</response>
        /// <response code="500">Erro interno ao processar a requisição</response>
        [HttpGet("pdf")]
        [Produces("application/pdf")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ObterRelatorioPdf()
        {
            var (bytes, contentType, nomeArquivo) = await _relatorioService.ExportarPdfAsync();
            return File(bytes, contentType, nomeArquivo);
        }

        /// <summary>
        /// Exporta a lista final de confirmados em PDF com checkbox de pagamento para preenchimento manual
        /// </summary>
        /// <returns>Arquivo PDF com lista de confirmados e coluna de pago (checkbox)</returns>
        /// <response code="200">Arquivo gerado com sucesso</response>
        /// <response code="500">Erro interno ao processar a requisição</response>
        [HttpGet("lista-final")]
        [Produces("application/pdf")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ObterListaFinalConfirmados()
        {
            var (bytes, contentType, nomeArquivo) = await _relatorioService.ExportarListaFinalConfirmadosPdfAsync();
            return File(bytes, contentType, nomeArquivo);
        }

        /// <summary>
        /// Exporta a relação de pessoas confirmadas com suas respectivas mesas em PDF
        /// </summary>
        /// <returns>Arquivo PDF com a relação pessoa x mesa</returns>
        /// <response code="200">Arquivo gerado com sucesso</response>
        /// <response code="500">Erro interno ao processar a requisição</response>
        [HttpGet("pessoa-mesa")]
        [Produces("application/pdf")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ObterRelacaoPessoaMesa()
        {
            var (bytes, contentType, nomeArquivo) = await _relatorioService.ExportarListaFinalConfirmadosPdfAsync();
            return File(bytes, contentType, nomeArquivo);
        }

    }
}
