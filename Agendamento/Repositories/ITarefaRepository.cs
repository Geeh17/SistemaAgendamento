using Agendamento.Models;

namespace Agendamento.Repositories
{
    /// <summary>
    /// Parâmetros de filtro e paginação usados na listagem de agendamentos.
    /// </summary>
    public class TarefaFiltro
    {
        public string? Titulo { get; set; }
        public StatusTarefa? Status { get; set; }
        public DateTime? Data { get; set; }
        public DateTime? DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public int Pagina { get; set; } = 1;
        public int TamanhoPagina { get; set; } = 20;
    }

    public interface ITarefaRepository
    {
        Task<Tarefa?> ObterPorIdAsync(int id);
        Task<(List<Tarefa> Itens, int Total)> ListarAsync(TarefaFiltro filtro);
        Task<Tarefa> AdicionarAsync(Tarefa tarefa);
        void Atualizar(Tarefa tarefa);
        void Remover(Tarefa tarefa);
        Task<bool> ExisteAsync(int id);
        Task<List<Tarefa>> ObterTodosParaEstatisticasAsync();
        Task SalvarAsync();
    }
}
