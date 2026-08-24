using System.Net;
using System.Text.Json;
using Agendamento.Services.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Agendamento.Middleware
{
    /// <summary>
    /// Captura exceções não tratadas em toda a pipeline e as converte em respostas
    /// HTTP consistentes no formato ProblemDetails (RFC 7807), evitando vazar
    /// stack traces para o cliente e centralizando o log de erros.
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IHostEnvironment _environment;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger,
            IHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var (statusCode, title) = exception switch
            {
                NotFoundException => (HttpStatusCode.NotFound, "Recurso não encontrado"),
                BusinessRuleException => (HttpStatusCode.UnprocessableEntity, "Regra de negócio violada"),
                AuthenticationFailedException => (HttpStatusCode.Unauthorized, "Falha na autenticação"),
                ArgumentException => (HttpStatusCode.BadRequest, "Requisição inválida"),
                _ => (HttpStatusCode.InternalServerError, "Erro interno do servidor")
            };

            if (statusCode == HttpStatusCode.InternalServerError)
            {
                _logger.LogError(exception, "Erro não tratado ao processar {Method} {Path}",
                    context.Request.Method, context.Request.Path);
            }
            else
            {
                _logger.LogWarning("{Title}: {Message} ({Method} {Path})",
                    title, exception.Message, context.Request.Method, context.Request.Path);
            }

            var problemDetails = new ProblemDetails
            {
                Status = (int)statusCode,
                Title = title,
                Detail = statusCode == HttpStatusCode.InternalServerError && !_environment.IsDevelopment()
                    ? "Ocorreu um erro inesperado. Tente novamente mais tarde."
                    : exception.Message,
                Instance = context.Request.Path
            };

            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = (int)statusCode;

            var json = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(json);
        }
    }
}
