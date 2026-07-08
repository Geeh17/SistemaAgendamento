namespace Agendamento.Services.Exceptions
{
    /// <summary>
    /// Lançada quando um recurso solicitado não existe.
    /// </summary>
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }
    }
}
