using Agendamento.Models;

namespace Agendamento.DTOs
{
    /// <summary>
    /// Dados de um agendamento retornados pela API.
    /// </summary>
    public class TarefaResponseDto
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public string? Local { get; set; }
        public DateTime DataAgendamento { get; set; }
        public DateTime DataCriacao { get; set; }
        public StatusTarefa Status { get; set; }
        public string StatusDescricao { get; set; } = string.Empty;

        public static TarefaResponseDto FromEntity(Tarefa tarefa)
        {
            return new TarefaResponseDto
            {
                Id = tarefa.Id,
                Titulo = tarefa.Titulo,
                Descricao = tarefa.Descricao,
                Local = tarefa.Local,
                DataAgendamento = tarefa.DataAgendamento,
                DataCriacao = tarefa.DataCriacao,
                Status = tarefa.Status,
                StatusDescricao = tarefa.Status.ToString()
            };
        }
    }

    /// <summary>
    /// Resumo estatístico usado pelo painel do front-end.
    /// </summary>
    public class TarefaEstatisticasDto
    {
        public int Total { get; set; }
        public int Pendentes { get; set; }
        public int Confirmados { get; set; }
        public int EmAndamento { get; set; }
        public int Concluidos { get; set; }
        public int Cancelados { get; set; }
        public int ProximosSeteDias { get; set; }
    }

    /// <summary>
    /// Envelope de resultado paginado.
    /// </summary>
    /// <typeparam name="T">Tipo dos itens da página.</typeparam>
    public class PagedResultDto<T>
    {
        public List<T> Itens { get; set; } = new();
        public int PaginaAtual { get; set; }
        public int TamanhoPagina { get; set; }
        public int TotalItens { get; set; }
        public int TotalPaginas => TamanhoPagina == 0 ? 0 : (int)Math.Ceiling(TotalItens / (double)TamanhoPagina);
    }
}
