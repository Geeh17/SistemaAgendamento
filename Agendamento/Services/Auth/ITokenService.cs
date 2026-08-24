using Agendamento.Models;

namespace Agendamento.Services.Auth
{
    public interface ITokenService
    {
        /// <summary>
        /// Gera um JWT assinado para o usuário informado.
        /// </summary>
        /// <returns>O token serializado e o instante (UTC) em que ele expira.</returns>
        (string Token, DateTime ExpiraEm) GerarToken(Usuario usuario);
    }
}
