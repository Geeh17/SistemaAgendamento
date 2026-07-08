using System.ComponentModel.DataAnnotations;
using Agendamento.Models;

namespace Agendamento.DTOs
{
    /// <summary>
    /// Dados recebidos para atualizar um agendamento existente.
    /// </summary>
    public class TarefaUpdateDto
    {
        [Required(ErrorMessage = "O título é obrigatório.")]
        [StringLength(150, ErrorMessage = "O título deve ter no máximo 150 caracteres.")]
        public string Titulo { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "A descrição deve ter no máximo 1000 caracteres.")]
        public string? Descricao { get; set; }

        [StringLength(150, ErrorMessage = "O local deve ter no máximo 150 caracteres.")]
        public string? Local { get; set; }

        [Required(ErrorMessage = "A data do agendamento é obrigatória.")]
        public DateTime DataAgendamento { get; set; }

        [Required(ErrorMessage = "O status é obrigatório.")]
        public StatusTarefa Status { get; set; }
    }
}
