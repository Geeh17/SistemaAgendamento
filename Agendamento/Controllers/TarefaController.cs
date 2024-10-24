using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Agendamento.Data;
using Agendamento.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Agendamento.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TarefaController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TarefaController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Tarefa>> GetTarefa(int id)
        {
            var tarefa = await _context.Tarefas.FindAsync(id);
            if (tarefa == null)
            {
                return NotFound();
            }
            return tarefa;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTarefa(int id, Tarefa tarefa)
        {
            if (id != tarefa.Id)
            {
                return BadRequest();
            }
            _context.Entry(tarefa).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TarefaExists(id))
                {
                    return NotFound();
                }
                throw;
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTarefa(int id)
        {
            var tarefa = await _context.Tarefas.FindAsync(id);
            if (tarefa == null)
            {
                return NotFound();
            }
            _context.Tarefas.Remove(tarefa);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("ObterTodos")]
        public async Task<ActionResult<IEnumerable<Tarefa>>> GetTarefas()
        {
            return await _context.Tarefas.ToListAsync();
        }

        [HttpGet("ObterPorTitulo")]
        public async Task<ActionResult<IEnumerable<Tarefa>>> GetTarefasPorTitulo(string titulo)
        {
            return await _context.Tarefas.Where(t => t.Status.Contains(titulo)).ToListAsync();
        }

        [HttpGet("ObterPorData")]
        public async Task<ActionResult<IEnumerable<Tarefa>>> GetTarefasPorData(DateTime data)
        {
            return await _context.Tarefas.Where(t => t.DataCriacao.Date == data.Date).ToListAsync();
        }

        [HttpGet("ObterPorStatus")]
        public async Task<ActionResult<IEnumerable<Tarefa>>> GetTarefasPorStatus(string status)
        {
            return await _context.Tarefas.Where(t => t.Status == status).ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<Tarefa>> CreateTarefa(Tarefa tarefa)
        {
            _context.Tarefas.Add(tarefa);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetTarefa), new { id = tarefa.Id }, tarefa);
        }

        private bool TarefaExists(int id)
        {
            return _context.Tarefas.Any(e => e.Id == id);
        }
    }
}
