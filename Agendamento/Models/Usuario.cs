using System.ComponentModel.DataAnnotations;

namespace Agendamento.Models
{
    /// <summary>
    /// Representa um usuário com acesso à API (autenticação/autorização via JWT).
    /// </summary>
    public class Usuario
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string NomeUsuario { get; set; } = string.Empty;

        /// <summary>
        /// Hash da senha gerado por <see cref="Microsoft.AspNetCore.Identity.PasswordHasher{TUser}"/>.
        /// A senha em texto puro nunca é armazenada.
        /// </summary>
        [Required]
        public string SenhaHash { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Papel { get; set; } = "Admin";

        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    }
}
