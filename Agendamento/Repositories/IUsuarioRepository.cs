using Agendamento.Models;

namespace Agendamento.Repositories
{
    public interface IUsuarioRepository
    {
        Task<Usuario?> ObterPorNomeUsuarioAsync(string nomeUsuario);
    }
}
