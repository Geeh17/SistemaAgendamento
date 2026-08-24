using System.ComponentModel.DataAnnotations;

namespace Agendamento.DTOs.Auth
{
    /// <summary>
    /// Credenciais enviadas para autenticação.
    /// </summary>
    public class LoginDto
    {
        [Required(ErrorMessage = "O nome de usuário é obrigatório.")]
        public string NomeUsuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "A senha é obrigatória.")]
        public string Senha { get; set; } = string.Empty;
    }
}
