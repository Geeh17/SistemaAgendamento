using Agendamento.DTOs;
using Agendamento.Models;
using Agendamento.Repositories;
using Agendamento.Services.Exceptions;

namespace Agendamento.Services
{
    public class TarefaService : ITarefaService
    {
        private readonly ITarefaRepository _repository;
        private readonly ILogger<TarefaService> _logger;

        public TarefaService(ITarefaRepository repository, ILogger<TarefaService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<TarefaResponseDto> ObterPorIdAsync(int id)
        {
            var tarefa = await _repository.ObterPorIdAsync(id)
                ?? throw new NotFoundException($"Não foi encontrado nenhum agendamento com o id {id}.");

            return TarefaResponseDto.FromEntity(tarefa);
        }

        public async Task<PagedResultDto<TarefaResponseDto>> ListarAsync(TarefaFiltro filtro)
        {
            var (itens, total) = await _repository.ListarAsync(filtro);

            return new PagedResultDto<TarefaResponseDto>
            {
                Itens = itens.Select(TarefaResponseDto.FromEntity).ToList(),
                PaginaAtual = filtro.Pagina < 1 ? 1 : filtro.Pagina,
                TamanhoPagina = filtro.TamanhoPagina is < 1 or > 200 ? 20 : filtro.TamanhoPagina,
                TotalItens = total
            };
        }

        public async Task<TarefaResponseDto> CriarAsync(TarefaCreateDto dto)
        {
            var tarefa = new Tarefa
            {
                Titulo = dto.Titulo.Trim(),
                Descricao = dto.Descricao?.Trim(),
                Local = dto.Local?.Trim(),
                DataAgendamento = dto.DataAgendamento,
                Status = dto.Status,
                DataCriacao = DateTime.UtcNow
            };

            await _repository.AdicionarAsync(tarefa);
            await _repository.SalvarAsync();

            _logger.LogInformation("Agendamento {Id} criado: {Titulo}", tarefa.Id, tarefa.Titulo);

            return TarefaResponseDto.FromEntity(tarefa);
        }

        public async Task<TarefaResponseDto> AtualizarAsync(int id, TarefaUpdateDto dto)
        {
            var tarefa = await _repository.ObterPorIdAsync(id)
                ?? throw new NotFoundException($"Não foi encontrado nenhum agendamento com o id {id}.");

            tarefa.Titulo = dto.Titulo.Trim();
            tarefa.Descricao = dto.Descricao?.Trim();
            tarefa.Local = dto.Local?.Trim();
            tarefa.DataAgendamento = dto.DataAgendamento;
            tarefa.Status = dto.Status;

            _repository.Atualizar(tarefa);
            await _repository.SalvarAsync();

            _logger.LogInformation("Agendamento {Id} atualizado", id);

            return TarefaResponseDto.FromEntity(tarefa);
        }

        public async Task RemoverAsync(int id)
        {
            var tarefa = await _repository.ObterPorIdAsync(id)
                ?? throw new NotFoundException($"Não foi encontrado nenhum agendamento com o id {id}.");

            _repository.Remover(tarefa);
            await _repository.SalvarAsync();

            _logger.LogInformation("Agendamento {Id} removido", id);
        }

        public async Task<TarefaEstatisticasDto> ObterEstatisticasAsync()
        {
            var todas = await _repository.ObterTodosParaEstatisticasAsync();
            var agora = DateTime.UtcNow;
            var limite = agora.AddDays(7);

            return new TarefaEstatisticasDto
            {
                Total = todas.Count,
                Pendentes = todas.Count(t => t.Status == StatusTarefa.Pendente),
                Confirmados = todas.Count(t => t.Status == StatusTarefa.Confirmado),
                EmAndamento = todas.Count(t => t.Status == StatusTarefa.EmAndamento),
                Concluidos = todas.Count(t => t.Status == StatusTarefa.Concluido),
                Cancelados = todas.Count(t => t.Status == StatusTarefa.Cancelado),
                ProximosSeteDias = todas.Count(t =>
                    t.DataAgendamento >= agora &&
                    t.DataAgendamento <= limite &&
                    t.Status != StatusTarefa.Cancelado)
            };
        }
    }
}
