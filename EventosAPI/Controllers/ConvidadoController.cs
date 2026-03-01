using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Eventos.Application.Interfaces;
using Eventos.Application.DTOs.Request;
using EventosAPI.Reports;

namespace EventosAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConvidadoController : ControllerBase
    {
        private readonly IEventoService _service;

        public ConvidadoController(IEventoService service)
        {
            _service = service;
        }

        /// <summary>
        /// Adiciona um novo convidado ao evento
        /// </summary>
        /// <param name="request">Dados do convidado a ser adicionado</param>
        /// <returns>Resposta com status da operação</returns>
        [HttpPost("adicionar")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AdicionarConvidado([FromBody] AdicionarConvidadoRequest request)
        {
            if (request == null)
                return StatusCode(400, new { mensagem = "Dados do convidado são obrigatórios" });

            var response = await _service.AdicionarConvidadoAsync(request);

            return StatusCode(response.CodigoStatus, response);
        }

        /// <summary>
        /// Verifica se um convidado já está cadastrado pelo nome
        /// </summary>
        /// <param name="nome">Nome do convidado a ser verificado</param>
        /// <returns>Booleano indicando se o convidado existe na base</returns>
        [HttpGet("verificar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> VerificarConvidadoAsync([FromQuery] string nome)
        {
            var response = await _service.VerificarConvidadoExisteAsync(nome);

            return StatusCode(response.CodigoStatus, response);
        }

        /// <summary>
        /// Zera todas as tabelas do banco de dados
        /// </summary>
        /// <returns>Resposta com status da operação</returns>
        [HttpDelete("zerar-tabelas")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ZerarTabelas()
        {
            var response = await _service.ZerarTabelasAsync();

            return StatusCode(response.CodigoStatus, response);
        }

        /// <summary>
        /// Retorna o relatório de convidados confirmados e o total de pessoas no evento
        /// </summary>
        /// <returns>Lista de convidados com acompanhantes e total de pessoas</returns>
        [HttpGet("relatorio")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ObterRelatorio()
        {
            var response = await _service.ObterRelatorioAsync();

            return StatusCode(response.CodigoStatus, response);
        }

        /// <summary>
        /// Exporta o relatório de convidados confirmados em formato Excel (.xlsx)
        /// </summary>
        [HttpGet("relatorio/excel")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ObterRelatorioExcel()
        {
            var response = await _service.ObterRelatorioAsync();

            if (response.CodigoStatus != 200)
                return StatusCode(response.CodigoStatus, response);

            var bytes = await RelatorioExcelGenerator.GerarAsync(response);
            var nomeArquivo = $"relatorio-convidados-{DateTime.Now:yyyy-MM-dd}.xlsx";

            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", nomeArquivo);
        }

        /// <summary>
        /// Exporta o relatório de convidados confirmados em formato PDF
        /// </summary>
        [HttpGet("relatorio/pdf")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ObterRelatorioPdf()
        {
            var response = await _service.ObterRelatorioAsync();

            if (response.CodigoStatus != 200)
                return StatusCode(response.CodigoStatus, response);

            var bytes = await RelatorioPdfGenerator.GerarAsync(response);
            var nomeArquivo = $"Relação de Participantes do Rodizio.pdf";

            return File(bytes, "application/pdf", nomeArquivo);
        }
    }
}
