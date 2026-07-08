using Agendamento.Repositories;
using Agendamento.Services;
using Microsoft.OpenApi.Models;

namespace Agendamento.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registra os repositórios e serviços da aplicação.
        /// </summary>
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<ITarefaRepository, TarefaRepository>();
            services.AddScoped<ITarefaService, TarefaService>();
            return services;
        }

        /// <summary>
        /// Configura o Swagger/OpenAPI com metadados descritivos da API.
        /// </summary>
        public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Sistema de Agendamento API",
                    Version = "v1",
                    Description = "API REST para gerenciamento de agendamentos, compromissos e tarefas."
                });
            });

            return services;
        }

        /// <summary>
        /// Configura a política de CORS usada pelo front-end.
        /// </summary>
        public static IServiceCollection AddCorsPolicy(this IServiceCollection services, string policyName)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(policyName, policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            return services;
        }
    }
}
