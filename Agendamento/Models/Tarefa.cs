using System;

namespace Agendamento.Models
{
    public class Tarefa
    {
       public int Id { get; set; }
    public string Nome { get; set; }
    public DateTime DataCriacao { get; set; }
    public string Status { get; set; } = string.Empty; // Inicializa com um valor padrão
    }
}
