using Agendamento.Data;
using Agendamento.Models;
using Microsoft.EntityFrameworkCore;

namespace Agendamento.Repositories
{
    public class TarefaRepository : ITarefaRepository
    {
        private readonly AppDbContext _context;

        public TarefaRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Tarefa?> ObterPorIdAsync(int id)
        {
            return await _context.Tarefas.FindAsync(id);
        }

        public async Task<(List<Tarefa> Itens, int Total)> ListarAsync(TarefaFiltro filtro)
        {
            var query = _context.Tarefas.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(filtro.Titulo))
            {
                query = query.Where(t => t.Titulo.Contains(filtro.Titulo));
            }

            if (filtro.Status.HasValue)
            {
                query = query.Where(t => t.Status == filtro.Status.Value);
            }

            if (filtro.Data.HasValue)
            {
                query = query.Where(t => t.DataAgendamento.Date == filtro.Data.Value.Date);
            }

            if (filtro.DataInicio.HasValue)
            {
                query = query.Where(t => t.DataAgendamento >= filtro.DataInicio.Value);
            }

            if (filtro.DataFim.HasValue)
            {
                query = query.Where(t => t.DataAgendamento <= filtro.DataFim.Value);
            }

            var total = await query.CountAsync();

            var pagina = filtro.Pagina < 1 ? 1 : filtro.Pagina;
            var tamanhoPagina = filtro.TamanhoPagina is < 1 or > 200 ? 20 : filtro.TamanhoPagina;

            var itens = await query
                .OrderBy(t => t.DataAgendamento)
                .Skip((pagina - 1) * tamanhoPagina)
                .Take(tamanhoPagina)
                .ToListAsync();

            return (itens, total);
        }

        public async Task<Tarefa> AdicionarAsync(Tarefa tarefa)
        {
            await _context.Tarefas.AddAsync(tarefa);
            return tarefa;
        }

        public void Atualizar(Tarefa tarefa)
        {
            _context.Entry(tarefa).State = EntityState.Modified;
        }

        public void Remover(Tarefa tarefa)
        {
            _context.Tarefas.Remove(tarefa);
        }

        public async Task<bool> ExisteAsync(int id)
        {
            return await _context.Tarefas.AnyAsync(t => t.Id == id);
        }

        public async Task<List<Tarefa>> ObterTodosParaEstatisticasAsync()
        {
            return await _context.Tarefas.AsNoTracking().ToListAsync();
        }

        public async Task SalvarAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
