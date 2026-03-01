using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Eventos.Application.Interfaces;
using Eventos.Application.DTOs.Request;

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
    }
}
