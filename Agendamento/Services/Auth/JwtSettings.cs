namespace Agendamento.Services.Auth
{
    /// <summary>
    /// Configurações do JWT lidas da seção "Jwt" do appsettings.json.
    /// </summary>
    public class JwtSettings
    {
        public const string SectionName = "Jwt";

        /// <summary>
        /// Chave secreta usada para assinar o token. Deve ter no mínimo 32 caracteres
        /// (256 bits) e, em produção, vir de uma variável de ambiente ou de um cofre de
        /// segredos — nunca de um valor fixo em código-fonte.
        /// </summary>
        public string Key { get; set; } = string.Empty;

        public string Issuer { get; set; } = string.Empty;

        public string Audience { get; set; } = string.Empty;

        public int ExpiracaoMinutos { get; set; } = 60;
    }
}
