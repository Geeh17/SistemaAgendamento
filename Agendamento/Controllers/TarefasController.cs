using Agendamento.DTOs;
using Agendamento.Models;
using Agendamento.Repositories;
using Agendamento.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Agendamento.Controllers
{
    /// <summary>
    /// Endpoints para gerenciamento de agendamentos (tarefas).
    /// Requer autenticação via JWT (ver <see cref="AuthController"/>).
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize]
    public class TarefasController : ControllerBase
    {
        private readonly ITarefaService _service;

        public TarefasController(ITarefaService service)
        {
            _service = service;
        }

        /// <summary>Lista agendamentos com filtros opcionais e paginação.</summary>
        /// <param name="titulo">Filtra por parte do título.</param>
        /// <param name="status">Filtra por status exato.</param>
        /// <param name="data">Filtra por uma data específica (ignora o horário).</param>
        /// <param name="dataInicio">Filtra a partir de uma data/hora.</param>
        /// <param name="dataFim">Filtra até uma data/hora.</param>
        /// <param name="pagina">Número da página (inicia em 1).</param>
        /// <param name="tamanhoPagina">Quantidade de itens por página (máx. 200).</param>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResultDto<TarefaResponseDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResultDto<TarefaResponseDto>>> Listar(
            [FromQuery] string? titulo,
            [FromQuery] StatusTarefa? status,
            [FromQuery] DateTime? data,
            [FromQuery] DateTime? dataInicio,
            [FromQuery] DateTime? dataFim,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanhoPagina = 20)
        {
            var filtro = new TarefaFiltro
            {
                Titulo = titulo,
                Status = status,
                Data = data,
                DataInicio = dataInicio,
                DataFim = dataFim,
                Pagina = pagina,
                TamanhoPagina = tamanhoPagina
            };

            var resultado = await _service.ListarAsync(filtro);
            return Ok(resultado);
        }

        /// <summary>Obtém um agendamento pelo id.</summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(TarefaResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TarefaResponseDto>> ObterPorId(int id)
        {
            var tarefa = await _service.ObterPorIdAsync(id);
            return Ok(tarefa);
        }

        /// <summary>Retorna estatísticas agregadas para o painel do sistema.</summary>
        [HttpGet("estatisticas")]
        [ProducesResponseType(typeof(TarefaEstatisticasDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<TarefaEstatisticasDto>> ObterEstatisticas()
        {
            var estatisticas = await _service.ObterEstatisticasAsync();
            return Ok(estatisticas);
        }

        /// <summary>Cria um novo agendamento.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(TarefaResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<TarefaResponseDto>> Criar([FromBody] TarefaCreateDto dto)
        {
            var criada = await _service.CriarAsync(dto);
            return CreatedAtAction(nameof(ObterPorId), new { id = criada.Id }, criada);
        }

        /// <summary>Atualiza um agendamento existente.</summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(TarefaResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TarefaResponseDto>> Atualizar(int id, [FromBody] TarefaUpdateDto dto)
        {
            var atualizada = await _service.AtualizarAsync(id, dto);
            return Ok(atualizada);
        }

        /// <summary>Remove um agendamento.</summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Remover(int id)
        {
            await _service.RemoverAsync(id);
            return NoContent();
        }
    }
}
