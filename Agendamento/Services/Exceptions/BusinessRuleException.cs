namespace Agendamento.Services.Exceptions
{
    /// <summary>
    /// Lançada quando uma requisição viola uma regra de negócio do domínio.
    /// </summary>
    public class BusinessRuleException : Exception
    {
        public BusinessRuleException(string message) : base(message) { }
    }
}
