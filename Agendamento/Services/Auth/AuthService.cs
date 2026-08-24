using Agendamento.DTOs.Auth;
using Agendamento.Models;
using Agendamento.Repositories;
using Agendamento.Services.Exceptions;
using Microsoft.AspNetCore.Identity;

namespace Agendamento.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly ITokenService _tokenService;
        private readonly IPasswordHasher<Usuario> _passwordHasher;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUsuarioRepository usuarioRepository,
            ITokenService tokenService,
            IPasswordHasher<Usuario> passwordHasher,
            ILogger<AuthService> logger)
        {
            _usuarioRepository = usuarioRepository;
            _tokenService = tokenService;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        public async Task<LoginResponseDto> AutenticarAsync(LoginDto dto)
        {
            var usuario = await _usuarioRepository.ObterPorNomeUsuarioAsync(dto.NomeUsuario.Trim());

            // Mensagem de erro genérica em ambos os casos (usuário inexistente ou senha
            // incorreta) para não revelar quais nomes de usuário existem no sistema.
            if (usuario is null)
            {
                _logger.LogWarning("Tentativa de login com usuário inexistente: {NomeUsuario}", dto.NomeUsuario);
                throw new AuthenticationFailedException("Usuário ou senha inválidos.");
            }

            var resultado = _passwordHasher.VerifyHashedPassword(usuario, usuario.SenhaHash, dto.Senha);

            if (resultado == PasswordVerificationResult.Failed)
            {
                _logger.LogWarning("Tentativa de login com senha incorreta para o usuário {NomeUsuario}", dto.NomeUsuario);
                throw new AuthenticationFailedException("Usuário ou senha inválidos.");
            }

            var (token, expiraEm) = _tokenService.GerarToken(usuario);

            _logger.LogInformation("Usuário {NomeUsuario} autenticado com sucesso", usuario.NomeUsuario);

            return new LoginResponseDto
            {
                Token = token,
                ExpiraEm = expiraEm,
                NomeUsuario = usuario.NomeUsuario,
                Papel = usuario.Papel
            };
        }
    }
}
