using Agendamento.DTOs.Auth;
using Agendamento.Services.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Agendamento.Controllers
{
    /// <summary>
    /// Endpoint de autenticação. Emite o JWT usado para acessar os demais endpoints da API.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>Autentica um usuário e retorna um JWT válido por um período limitado.</summary>
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginDto dto)
        {
            var resposta = await _authService.AutenticarAsync(dto);
            return Ok(resposta);
        }
    }
}
