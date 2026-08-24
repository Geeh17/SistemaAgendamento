using Agendamento.DTOs.Auth;

namespace Agendamento.Services.Auth
{
    public interface IAuthService
    {
        /// <summary>
        /// Valida as credenciais informadas e, se corretas, retorna um JWT.
        /// Lança <see cref="Exceptions.AuthenticationFailedException"/> caso sejam inválidas.
        /// </summary>
        Task<LoginResponseDto> AutenticarAsync(LoginDto dto);
    }
}
