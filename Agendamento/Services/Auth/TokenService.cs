using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Agendamento.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Agendamento.Services.Auth
{
    public class TokenService : ITokenService
    {
        private readonly JwtSettings _settings;

        public TokenService(IOptions<JwtSettings> settings)
        {
            _settings = settings.Value;
        }

        public (string Token, DateTime ExpiraEm) GerarToken(Usuario usuario)
        {
            var expiraEm = DateTime.UtcNow.AddMinutes(_settings.ExpiracaoMinutos);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, usuario.NomeUsuario),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.NomeUsuario),
                new Claim(ClaimTypes.Role, usuario.Papel)
            };

            var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
            var credenciais = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: claims,
                expires: expiraEm,
                signingCredentials: credenciais);

            var tokenSerializado = new JwtSecurityTokenHandler().WriteToken(token);

            return (tokenSerializado, expiraEm);
        }
    }
}
