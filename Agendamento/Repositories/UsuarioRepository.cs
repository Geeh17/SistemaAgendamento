using Agendamento.Data;
using Agendamento.Models;
using Microsoft.EntityFrameworkCore;

namespace Agendamento.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly AppDbContext _context;

        public UsuarioRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Usuario?> ObterPorNomeUsuarioAsync(string nomeUsuario)
        {
            return await _context.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.NomeUsuario == nomeUsuario);
        }
    }
}
