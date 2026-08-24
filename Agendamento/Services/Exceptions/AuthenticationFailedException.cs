namespace Agendamento.Services.Exceptions
{
    /// <summary>
    /// Lançada quando as credenciais informadas no login são inválidas.
    /// </summary>
    public class AuthenticationFailedException : Exception
    {
        public AuthenticationFailedException(string message) : base(message) { }
    }
}
