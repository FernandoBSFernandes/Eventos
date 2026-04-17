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

        public AdministracaoController(IAdministracaoService administracaoService)
        {
            _administracaoService = administracaoService;
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

    }
}
