using Agendamento.DTOs;
using Agendamento.Repositories;

namespace Agendamento.Services
{
    public interface ITarefaService
    {
        Task<TarefaResponseDto> ObterPorIdAsync(int id);
        Task<PagedResultDto<TarefaResponseDto>> ListarAsync(TarefaFiltro filtro);
        Task<TarefaResponseDto> CriarAsync(TarefaCreateDto dto);
        Task<TarefaResponseDto> AtualizarAsync(int id, TarefaUpdateDto dto);
        Task RemoverAsync(int id);
        Task<TarefaEstatisticasDto> ObterEstatisticasAsync();
    }
}
