using System.ComponentModel.DataAnnotations;

namespace Agendamento.Models
{
    /// <summary>
    /// Representa um agendamento (compromisso/tarefa) dentro do sistema.
    /// </summary>
    public class Tarefa
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O título é obrigatório.")]
        [StringLength(150, ErrorMessage = "O título deve ter no máximo 150 caracteres.")]
        public string Titulo { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "A descrição deve ter no máximo 1000 caracteres.")]
        public string? Descricao { get; set; }

        [StringLength(150, ErrorMessage = "O local deve ter no máximo 150 caracteres.")]
        public string? Local { get; set; }

        /// <summary>
        /// Data e hora em que o compromisso está agendado para acontecer.
        /// </summary>
        [Required(ErrorMessage = "A data do agendamento é obrigatória.")]
        public DateTime DataAgendamento { get; set; }

        /// <summary>
        /// Data e hora em que o registro foi criado no sistema (preenchido automaticamente).
        /// </summary>
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

        public StatusTarefa Status { get; set; } = StatusTarefa.Pendente;
    }
}
