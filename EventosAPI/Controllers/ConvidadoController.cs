using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Eventos.Application.Interfaces;
using Eventos.Application.DTOs.Request;
using Eventos.Application.DTOs.Response;

namespace EventosAPI.Controllers
{
    /// <summary>
    /// Gerenciamento de convidados do evento.
    /// Permite adicionar, listar, verificar, remover convidados e consultar vagas restantes.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Consumes("application/json")]
    [Produces("application/json")]
    [Tags("Convidados")]
    public class ConvidadoController : ControllerBase
    {
        private readonly IConvidadoService _convidadoService;

        public ConvidadoController(IConvidadoService convidadoService)
        {
            _convidadoService = convidadoService;
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
        [ProducesResponseType(typeof(ListarConvidadosResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ListarConvidados()
        {
            var response = await _convidadoService.ListarConvidadosAsync();

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
        /// Retorna a quantidade de vagas restantes no evento
        /// </summary>
        /// <returns>Vagas restantes e total de pessoas já confirmadas</returns>
        /// <response code="200">Consulta realizada com sucesso</response>
        /// <response code="500">Erro interno ao processar a requisição</response>
        [HttpGet("vagas-restantes")]
        [ProducesResponseType(typeof(VagasRestantesResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ObterVagasRestantes()
        {
            var response = await _convidadoService.ObterVagasRestantesAsync();

            return StatusCode(response.CodigoStatus, response);
        }
    }
}
