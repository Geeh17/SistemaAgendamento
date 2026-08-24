namespace Agendamento.DTOs.Auth
{
    /// <summary>
    /// Resposta retornada após uma autenticação bem-sucedida.
    /// </summary>
    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiraEm { get; set; }
        public string NomeUsuario { get; set; } = string.Empty;
        public string Papel { get; set; } = string.Empty;
    }
}
