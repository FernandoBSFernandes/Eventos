using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Eventos.Application.Interfaces;
using Eventos.Application.DTOs.Response;

namespace EventosAPI.Controllers
{
    /// <summary>
    /// Operações administrativas do evento.
    /// Permite zerar todas as tabelas e remover registros duplicados.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Consumes("application/json")]
    [Produces("application/json")]
    [Tags("Administração")]
    public class AdministracaoController : ControllerBase
    {
        private readonly IAdministracaoService _administracaoService;
        private readonly IMigracaoDadosService _migracaoDadosService;

        public AdministracaoController(
            IAdministracaoService administracaoService,
            IMigracaoDadosService migracaoDadosService)
        {
            _administracaoService = administracaoService;
            _migracaoDadosService = migracaoDadosService;
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
        /// Migra convidados e acompanhantes da base de origem para a base de destino.
        /// Registros com o mesmo nome já existentes no destino são ignorados.
        /// </summary>
        /// <returns>Quantidade de convidados e acompanhantes migrados</returns>
        /// <response code="200">Migração concluída com sucesso</response>
        /// <response code="500">Erro interno ao processar a requisição</response>
        [HttpPost("migrar-dados")]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> MigrarDados()
        {
            try
            {
                var (convidados, acompanhantes) = await _migracaoDadosService.MigrarDadosAsync();

                return Ok(new BaseResponse(200,
                    $"Migração concluída. Convidados migrados: {convidados}. Acompanhantes migrados: {acompanhantes}."));
            }
            catch (Exception)
            {
                return StatusCode(500, new BaseResponse(500, "Ocorreu um erro interno durante a migração."));
            }
        }
    }
}
