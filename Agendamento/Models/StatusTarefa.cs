namespace Agendamento.Models
{
    /// <summary>
    /// Representa o estágio atual de um agendamento dentro do fluxo de trabalho.
    /// </summary>
    public enum StatusTarefa
    {
        Pendente = 0,
        Confirmado = 1,
        EmAndamento = 2,
        Concluido = 3,
        Cancelado = 4
    }
}
